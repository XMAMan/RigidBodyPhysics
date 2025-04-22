using GraphicMinimal;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse
{
    internal class ResolverHelper
    {
        public static void MoveBodiesWithConstraint(List<IRigidBody> bodies, CollisionPointWithImpulse[] collisions, float dt, SolverSettings settings)
        {
            //Schritt 1: Erzeuge ForceDirection, ImpulseMass und Bias für jedes Constraint-Objekt
            float invDt = dt > 0.0f ? 1.0f / dt : 0.0f;
            var constraints = new ConstraintFactory().CreateConstraints(new ConstraintConstructorData()
            {
                Bodies = bodies,
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
            for (int i=0;i<settings.IterationCount;i++)
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
