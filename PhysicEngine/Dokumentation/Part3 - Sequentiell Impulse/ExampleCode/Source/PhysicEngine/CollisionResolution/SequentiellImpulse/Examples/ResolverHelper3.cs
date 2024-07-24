using GraphicMinimal;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Examples
{
    //Ist das gleiche wie ResolverHelper nur ohne WarmStart. Hier wird untersucht, an welcher Stelle die Anwendung der
    //externen Kraft erfolgen muss.
    internal class ResolverHelper3
    {
        public enum Variation { Variation1, Variation2, Variation3 };

        public static float[] MoveBodiesWithConstraint(List<IRigidBody> bodies, CollisionPointWithImpulse[] collisions, 
            float dt, SolverSettings settings, Variation variation)
        {
            //Möglichkeit 1 -> Geht nicht
            if (variation == Variation.Variation1) ApplyExternalForce(bodies, dt);

            //Erzeuge ForceDirection, ImpulseMass und Bias für jedes Constraint-Objekt
            float invDt = dt > 0.0f ? 1.0f / dt : 0.0f;
            var constraints = new ConstraintFactory().CreateConstraints(new ConstraintConstructorData()
            {
                Bodies = bodies,
                Settings = settings,
                Dt = dt,
                InvDt = invDt
            },
                collisions).ToList();

            float[] impulseSum = new float[constraints.Count]; //Summe der Impulse pro Constraint

            //Möglichkeit 2: -> Geht
            if (variation == Variation.Variation2) ApplyExternalForce(bodies, dt);

            //Finde per PGS Relative-Kontaktpunktgeschwindigkeitswerte, welche dem Bias-Wert entsprechen
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

                    float impulse = c.ImpulseMass * (c.Bias - velocityInForceDirection);

                    // Clamp the accumulated impulse
                    float oldSum = c.AccumulatedImpulse;
                    c.AccumulatedImpulse = MathHelp.Clamp(oldSum + impulse, c.MinImpulse, c.MaxImpulse);
                    impulse = c.AccumulatedImpulse - oldSum;

                    impulseSum[constraints.IndexOf(c)] += impulse;
                    ApplyImpulse(c, impulse);
                }
            }

            //Möglichkeit 3:  -> Geht nicht
            if (variation == Variation.Variation3) ApplyExternalForce(bodies, dt);

            return impulseSum;
        }

        private static void ApplyExternalForce(List<IRigidBody> bodies, float dt)
        {
            //Wende die externe Kraft für alle Körper an
            foreach (var body in bodies)
            {
                body.Velocity.X += body.InverseMass * body.Force.X * dt;
                body.Velocity.Y += body.InverseMass * body.Force.Y * dt;
                body.AngularVelocity += body.InverseInertia * body.Torque * dt;
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
