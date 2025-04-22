using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse
{
    internal class ResolverHelper
    {
        internal static void MoveBodiesWithConstraint(List<IRigidBody> bodies, List<IJoint> joints, CollisionPointWithImpulse[] collisions, MouseConstraintData mouseData, float dt, SolverSettings settings)
        {
            //Schritt 1: Erzeuge ForceDirection, ImpulseMass und Bias für jedes Constraint-Objekt
            float invDt = dt > 0.0f ? 1.0f / dt : 0.0f;
            var constraints = new ConstraintFactory().CreateConstraints(new ConstraintConstructorData()
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
                    c.ApplyWarmStartImpulse();
                }
            }


            //Schritt 4: Finde per PGS Relative-Kontaktpunktgeschwindigkeitswerte, welche dem Bias-Wert entsprechen
            for (int i=0;i<settings.IterationCount;i++)
            {
                foreach (var c in constraints)
                {
                    c.DoSingleSIStep();
                }
            }
        }

        internal static void ApplyWarmStartImpulse(ILinear1DConstraint c)
        {
            ApplyLinearImpulse(c, c.ForceDirection * c.AccumulatedImpulse);
        }

        internal static void DoSingleSIStepStiff(ILinear1DConstraint c)
        {
            //VelocityAtContactPoint = V + Cross([0, 0, AngularVelocity], [R.X, R.Y, 0])
            Vec2D v1 = c.B1.Velocity + new Vec2D(-c.B1.AngularVelocity * c.R1.Y, c.B1.AngularVelocity * c.R1.X);//=v1 + Cross([0, 0, AngularVelocity1], [R1.X, R1.Y, 0])
            Vec2D v2 = c.B2.Velocity + new Vec2D(-c.B2.AngularVelocity * c.R2.Y, c.B2.AngularVelocity * c.R2.X);//=v2 + Cross([0, 0, AngularVelocity2], [R2.X, R2.Y, 0])
            Vec2D relativeVelocity = v2 - v1;

            // Relative velocity in Force direction
            float velocityInForceDirection = relativeVelocity * c.ForceDirection;

            // impulse = Integral F dt = m * delta-v
            // delta-v = impulse / m
            float impulse = c.ImpulseMass * (c.Bias - velocityInForceDirection);


            // Clamp the accumulated impulse
            float oldSum = c.AccumulatedImpulse;
            c.AccumulatedImpulse = MathHelp.Clamp(oldSum + impulse, c.MinImpulse, c.MaxImpulse);
            impulse = c.AccumulatedImpulse - oldSum;

            ApplyLinearImpulse(c, impulse * c.ForceDirection);

            c.SaveImpulse();
        }

        internal static void DoSingleSIStepSoft(ISoftConstraint1D c)
        {
            if (c.IsSoftConstraint == false)
            {
                DoSingleSIStepStiff(c);
                return;
            }

            //VelocityAtContactPoint = V + Cross([0, 0, AngularVelocity], [R.X, R.Y, 0])
            Vec2D v1 = c.B1.Velocity + new Vec2D(-c.B1.AngularVelocity * c.R1.Y, c.B1.AngularVelocity * c.R1.X);//=v1 + Cross([0, 0, AngularVelocity1], [R1.X, R1.Y, 0])
            Vec2D v2 = c.B2.Velocity + new Vec2D(-c.B2.AngularVelocity * c.R2.Y, c.B2.AngularVelocity * c.R2.X);//=v2 + Cross([0, 0, AngularVelocity2], [R2.X, R2.Y, 0])
            Vec2D relativeVelocity = v2 - v1;

            // Relative velocity in Force direction
            float velocityInForceDirection = relativeVelocity * c.ForceDirection;

            //Quelle1: https://github.com/erincatto/box2d/blob/main/src/dynamics/b2_distance_joint.cpp#L191
            //Quelle2: 3D Constraint Derivations for Impulse Solvers - Marijn 2015" Seite 9 Formel (2.17)
            float impulse = -c.ImpulseMass * (velocityInForceDirection + c.Beta * c.PositionError + c.Gamma * c.AccumulatedImpulse);


            // Clamp the accumulated impulse
            float oldSum = c.AccumulatedImpulse;
            c.AccumulatedImpulse = MathHelp.Clamp(oldSum + impulse, c.MinImpulse, c.MaxImpulse);
            impulse = c.AccumulatedImpulse - oldSum;

            ApplyLinearImpulse(c, impulse * c.ForceDirection);

            c.SaveImpulse();
        }

        internal static void DoSingleSIStepStiff(ILinear2DConstraint c)
        {
            Vec2D impulse = c.InverseK * (-c.Bias - c.GetCDot());

            Vec2D oldSum = c.AccumulatedImpulse;
            c.AccumulatedImpulse += impulse;

            //Max-Clamping
            if (c.MaxImpulse != float.MaxValue && c.AccumulatedImpulse.SquareLength() > c.MaxImpulse * c.MaxImpulse)
            {
                c.AccumulatedImpulse *= c.MaxImpulse / c.AccumulatedImpulse.Length();
            }

            impulse = c.AccumulatedImpulse - oldSum;

            ApplyLinearImpulse(c, impulse);

            c.SaveImpulse();
        }

        internal static void DoSingleSIStepSoft(ISoftConstraint2D c)
        {
            if (c.IsSoftConstraint == false)
            {
                DoSingleSIStepStiff(c);
                return;
            }

            Vec2D impulse = c.InverseK * (-c.GetCDot() - c.Gamma * c.AccumulatedImpulse - c.Beta * c.PositionError);

            Vec2D oldSum = c.AccumulatedImpulse;
            c.AccumulatedImpulse += impulse;

            //Max-Clamping
            if (c.MaxImpulse != float.MaxValue && c.AccumulatedImpulse.SquareLength() > c.MaxImpulse * c.MaxImpulse)
            {
                c.AccumulatedImpulse *= c.MaxImpulse / c.AccumulatedImpulse.Length();
            }

            impulse = c.AccumulatedImpulse - oldSum;

            ApplyLinearImpulse(c, impulse);

            c.SaveImpulse();
        }

        internal static void DoSingleSIStepForAngularImpulseStiff(IAngularConstraint c)
        {
            float angularVelocity = c.B2.AngularVelocity - c.B1.AngularVelocity;

            float angularImpulse = c.ImpulseMass * (c.Bias - angularVelocity);

            float oldSum = c.AccumulatedImpulse;
            c.AccumulatedImpulse = MathHelp.Clamp(oldSum + angularImpulse, c.MinImpulse, c.MaxImpulse);
            angularImpulse = c.AccumulatedImpulse - oldSum;

            ApplyAngularImpulse(c, angularImpulse);

            //Wenn ich das so machen, dann Schwebt ein Objekt auf einmal weil es durch Zauberhand aus dem Nichts einen linearne Impuls erfährt
            //Vec2D linearImpulse1 = c.R1.Spin90() * angularImpulse / (c.R1 * c.R1);
            //c.B1.Velocity += linearImpulse1 * c.B1.InverseMass;

            //Vec2D linearImpulse2 = c.R2.Spin90() * angularImpulse / (c.R2 * c.R2);
            //c.B2.Velocity -= linearImpulse2 * c.B2.InverseMass;

            c.SaveImpulse();
        }

        internal static void DoSingleSIStepForAngularImpulseSoft(ISoftAngular c)
        {
            if (c.IsSoftConstraint == false)
            {
                DoSingleSIStepForAngularImpulseStiff(c);
                return;
            }

            float angularVelocity = c.B2.AngularVelocity - c.B1.AngularVelocity;

            float angularImpulse = -c.ImpulseMass * (angularVelocity + c.Beta * c.PositionError + c.Gamma * c.AccumulatedImpulse);

            float oldSum = c.AccumulatedImpulse;
            c.AccumulatedImpulse = MathHelp.Clamp(oldSum + angularImpulse, c.MinImpulse, c.MaxImpulse);
            angularImpulse = c.AccumulatedImpulse - oldSum;

            ApplyAngularImpulse(c, angularImpulse);

            c.SaveImpulse();
        }

        //Impulse = m * deltaV -> Impulse/m = deltaV
        internal static void ApplyLinearImpulse(IConstraint c, Vec2D impulse)
        {
            if (c.B1 != null)
            {
                c.B1.Velocity -= impulse * c.B1.InverseMass;
                c.B1.AngularVelocity -= Vec2D.ZValueFromCross(c.R1, impulse) * c.B1.InverseInertia;
            }            

            if (c.B2 != null)
            {
                c.B2.Velocity += impulse * c.B2.InverseMass;
                c.B2.AngularVelocity += Vec2D.ZValueFromCross(c.R2, impulse) * c.B2.InverseInertia;
            }            
        }

        internal static void ApplyAngularImpulse(IConstraint c, float angularImpulse)
        {
            c.B1.AngularVelocity -= angularImpulse * c.B1.InverseInertia;
            c.B2.AngularVelocity += angularImpulse * c.B2.InverseInertia;
        }
    }
}
