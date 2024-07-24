using GraphicMinimal;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Examples
{
    //Hier wird gezeigt, wie man eine Feder simuliert, die ohne DistanceJointConstraint arbeitet aber dafür mit externer Kraft
    //Bei dieser Art der Simulation schwingt eine ungedämpfte Feder unendlich
    internal class ResolverHelper1
    {
        public static void MoveBodiesWithConstraint(List<IRigidBody> bodies, List<IJoint> joints, CollisionPointWithImpulse[] collisions, MouseConstraintData mouseData, float dt, SolverSettings settings)
        {
            //Schritt 0: Setze die Body.Force-Property laut Federkraftformel
            foreach (var joint in joints) 
                GetForceFromDistanceJoint(joint as DistanceJoint); 


            //Schritt 1: Erzeuge ForceDirection, ImpulseMass und Bias für jedes Constraint-Objekt
            float invDt = dt > 0.0f ? 1.0f / dt : 0.0f;
            var constraints = CreateConstraints(new ConstraintConstructorData()
            {
                Bodies = bodies,
                Joints = joints,
                MouseData = mouseData,
                Settings = settings,
                Dt = dt,
                InvDt = invDt
            },
                collisions);

            //Schritt 2: Wende die externe Kraft für alle Körper an
            foreach (var body in bodies)
            {
                body.Velocity.X += body.InverseMass * body.Force.X * dt;
                body.Velocity.Y += body.InverseMass * body.Force.Y * dt;
                body.AngularVelocity += body.InverseInertia * body.Torque * dt;
            }

            //Schritt 3: Wende den ersten Impuls an, welcher bereits den Großteil der Korrektur erreichen sollte
            if (settings.DoWarmStart)
            {
                foreach (var c in constraints)
                {
                    ApplyImpulse(c, c.AccumulatedImpulse);
                }
            }


            //Schritt 4: Finde per PGS Relative-Kontaktpunktgeschwindigkeitswerte, welche dem Bias-Wert entsprechen
            for (int i = 0; i < settings.IterationCount; i++)
            {
                foreach (var c in constraints)
                {
                    //VelocityAtContactPoint = V + mAngularVelocity cross R
                    Vector2D v1 = c.B1.Velocity + new Vector2D(-c.B1.AngularVelocity * c.R1.Y, c.B1.AngularVelocity * c.R1.X);
                    Vector2D v2 = c.B2.Velocity + new Vector2D(-c.B2.AngularVelocity * c.R2.Y, c.B2.AngularVelocity * c.R2.X);
                    Vector2D relativeVelocity = v2 - v1;

                    // Relative velocity in Force direction
                    float velocityInForceDirection = relativeVelocity * c.ForceDirection;

                    // impulse = Integral F dt = m * delta-v
                    // delta-v = impulse / m
                    float impulse = c.ImpulseMass * (c.Bias - velocityInForceDirection);

                    // Clamp the accumulated impulse
                    float oldSum = c.AccumulatedImpulse;
                    c.AccumulatedImpulse = MathHelp.Clamp(oldSum + impulse, c.MinImpulse, c.MaxImpulse);
                    impulse = c.AccumulatedImpulse - oldSum;

                    ApplyImpulse(c, impulse);

                    c.SaveImpulse();
                }
            }
        }

        //Diese Funktion zeigt, das man eine Feder auch darüber simulieren kann, indem man sie als externe Kraft betrachtet
        private static void GetForceFromDistanceJoint(DistanceJoint joint)
        {
            Vector2D a1Toa2 = joint.Anchor2 - joint.Anchor1;
            float length = a1Toa2.Length();
            Vector2D ForceDirection = length > 0.0001f ? a1Toa2 / length : new Vector2D(1, 0);

            Vector2D R1 = joint.Anchor1 - joint.B1.Center;
            Vector2D R2 = joint.Anchor2 - joint.B2.Center;

            //VelocityAtContactPoint = V + mAngularVelocity cross R
            Vector2D v1 = joint.B1.Velocity + new Vector2D(-joint.B1.AngularVelocity * R1.Y, joint.B1.AngularVelocity * R1.X);
            Vector2D v2 = joint.B2.Velocity + new Vector2D(-joint.B2.AngularVelocity * R2.Y, joint.B2.AngularVelocity * R2.X);
            Vector2D relativeVelocity = v2 - v1;

            // Relative velocity in Force direction
            float velocityInForceDirection = relativeVelocity * ForceDirection;

            float force = -joint.Damping * velocityInForceDirection - joint.Stiffness * (length - joint.Length);

            joint.B1.Force -= ForceDirection * force;
            joint.B2.Force += ForceDirection * force;
        }

        private static IConstraint[] CreateConstraints(ConstraintConstructorData data, CollisionPointWithImpulse[] collisions)
        {
            List<IConstraint> constraints = new List<IConstraint>();

            constraints.AddRange(collisions.Select(x => new NormalConstraint(data, x)));
            constraints.AddRange(collisions.Select(x => new FrictionConstraint(data, x)));

            return constraints.ToArray();
        }


        //Impulse = m * deltaV -> Impulse/m = deltaV
        private static void ApplyImpulse(IConstraint c, float impulse)
        {
            Vector2D impulseVector = impulse * c.ForceDirection;
            c.B1.Velocity -= impulseVector * c.B1.InverseMass;
            c.B1.AngularVelocity -= Vector2D.ZValueFromCross(c.R1, impulseVector) * c.B1.InverseInertia;

            c.B2.Velocity += impulseVector * c.B2.InverseMass;
            c.B2.AngularVelocity += Vector2D.ZValueFromCross(c.R2, impulseVector) * c.B2.InverseInertia;
        }
    }
}
