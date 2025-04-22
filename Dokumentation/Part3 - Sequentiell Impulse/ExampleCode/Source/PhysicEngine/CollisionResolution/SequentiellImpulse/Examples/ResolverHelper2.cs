using GraphicMinimal;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Examples
{
    //Erweitert ResolverHelper1 um die Nutzung der FrictionConstraint
    //Hier wird untersucht was passiert wenn man einmal direkt die Impulse-Variable clampt und dann, wenn
    //wenn die AccumulatedImpulse-Variable stattdessen benutzt
    internal class ResolverHelper2
    {
        private static string Log = "";

        public static float[] MoveBodiesWithConstraint(List<IRigidBody> bodies, CollisionPointWithImpulse[] collisions, float dt, SolverSettings settings, bool useAccumulatedImpulse)
        {
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

            for (int i = 0; i < settings.IterationCount; i++)
            {
                foreach (var c in constraints)
                {
                    Vector2D v1 = c.B1.Velocity + new Vector2D(-c.B1.AngularVelocity * c.R1.Y, c.B1.AngularVelocity * c.R1.X);
                    Vector2D v2 = c.B2.Velocity + new Vector2D(-c.B2.AngularVelocity * c.R2.Y, c.B2.AngularVelocity * c.R2.X);
                    Vector2D relativeVelocity = v2 - v1;
                    float velocityInForceDirection = relativeVelocity * c.ForceDirection;
                    float impulse = c.ImpulseMass * (c.Bias - velocityInForceDirection);

                    if (useAccumulatedImpulse)
                    {
                        // Clamp the accumulated impulse
                        float oldSum = c.AccumulatedImpulse;
                        c.AccumulatedImpulse = MathHelp.Clamp(oldSum + impulse, c.MinImpulse, c.MaxImpulse);
                        impulse = c.AccumulatedImpulse - oldSum;
                    }
                    else
                    {
                        impulse = MathHelp.Clamp(impulse, c.MinImpulse, c.MaxImpulse);
                    }

                    impulseSum[constraints.IndexOf(c)] += impulse;

                    //Logging für FrictionA
                    //if (constraints.IndexOf(c) == 2) //Frictionimpuls für den oberen Kollisionspunkt
                    //{
                    //    float sum = impulseSum[constraints.IndexOf(c)];
                    //    Log += i + "\t" + (unclampedImpulse * 10000) + "\t" + (impulse * 10000) + "\t" + (sum * 10000) + "\r\n";
                    //}

                    //Logging für alle 4 Constraints
                    {
                        string constraintName = new[] { "NormalA", "NormalB", "FrictionA", "FrictionB" }[constraints.IndexOf(c)];
                        Log += i + "\t" + constraintName + "\t" + (impulse * 10000) + "\t" + (c.AccumulatedImpulse * 10000) + "\t" + velocityInForceDirection + "\t" + c.B1.Velocity.ToString() + "\t" + c.B1.AngularVelocity + "\r\n";
                    }

                    ApplyImpulse(c, impulse);
                }
            }

            return impulseSum;
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
