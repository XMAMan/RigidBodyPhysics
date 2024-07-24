using GraphicMinimal;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse
{
    internal class ResolverHelper
    {
        public static void MoveBodiesWithConstraint(List<IRigidBody> bodies, List<IJoint> joints, CollisionPointWithImpulse[] collisions, MouseConstraintData mouseData, float dt, SolverSettings settings)
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
                    if (c.IsMultiConstraint == false)
                        ApplyImpulse(c, c.AccumulatedImpulse);
                    else
                        ApplyImpulse(c, c.AccumulatedMultiConstraintImpulse);
                }
            }


            //Schritt 4: Finde per PGS Relative-Kontaktpunktgeschwindigkeitswerte, welche dem Bias-Wert entsprechen
            for (int i=0;i<settings.IterationCount;i++)
            {
                foreach (var c in constraints)
                {
                    if (c.IsMultiConstraint == false)
                        SolveSingleConstraint(c);
                    else
                        SolveMultiConstraint(c, invDt);
                }
            }
        }

        private static void SolveSingleConstraint(IConstraint c)
        {
            //VelocityAtContactPoint = V + mAngularVelocity cross R
            Vector2D v1 = c.B1.Velocity + new Vector2D(-c.B1.AngularVelocity * c.R1.Y, c.B1.AngularVelocity * c.R1.X);
            Vector2D v2 = c.B2.Velocity + new Vector2D(-c.B2.AngularVelocity * c.R2.Y, c.B2.AngularVelocity * c.R2.X);
            Vector2D relativeVelocity = v2 - v1;

            // Relative velocity in Force direction
            float velocityInForceDirection = relativeVelocity * c.ForceDirection;

            float impulse = float.NaN;
            if (c.IsSoftConstraint == false)
            {
                // impulse = Integral F dt = m * delta-v
                // delta-v = impulse / m
                impulse = c.ImpulseMass * (c.Bias - velocityInForceDirection);
            }
            else
            {
                var d = (c as DistanceJointConstraint);
                //So könnte man laut der Formel (2.13) aus "3D Constraint Derivations for Impulse Solvers - Marijn 2015" auch den Impuls berechnen -> Hier darf ich kein Warmstart verwenden
                //impulse = (-velocityInForceDirection - d.BetaCDeltaT) / d.K / settings.IterationCount;

                //(2.17)-Formel aus "3D Constraint Derivations for Impulse Solvers - Marijn 2015"
                impulse = (-velocityInForceDirection - d.GammaDt * c.AccumulatedImpulse - d.BetaCDeltaT) / d.K;

                //Quelle: https://github.com/erincatto/box2d/blob/main/src/dynamics/b2_distance_joint.cpp#L191
                //Diese Formel entspricht auch der Formel von "3D Constraint Derivations for Impulse Solvers - Marijn 2015" Seite 9 Formel (2.17)
                //impulse = -c.ImpulseMass * (velocityInForceDirection + c.Bias + c.Gamma * c.AccumulatedImpulse);

            }


            // Clamp the accumulated impulse
            float oldSum = c.AccumulatedImpulse;
            c.AccumulatedImpulse = MathHelp.Clamp(oldSum + impulse, c.MinImpulse, c.MaxImpulse);
            impulse = c.AccumulatedImpulse - oldSum;

            ApplyImpulse(c, impulse);

            c.SaveImpulse();
        }

        private static void SolveMultiConstraint(IConstraint c, float invDt)
        {
            Vector2D impulse = null;

            if (c.IsSoftConstraint == false)
            {
                float biasFactor = 1.0f; //0..1 -> Wie schnell folgt das Objekt dem Mauszeiger
                Vector2D bias = biasFactor * invDt * c.GetC();
                impulse = c.InverseK * (-bias - c.GetCDot());
            }else
            {
                impulse = c.InverseK * (-c.GetCDot() - c.Gamma * c.AccumulatedMultiConstraintImpulse - c.Beta * c.GetC());
            }            

            Vector2D oldSum = c.AccumulatedMultiConstraintImpulse;
            c.AccumulatedMultiConstraintImpulse += impulse;

            //Max-Clamping
            if (c.MaxImpulse != float.MaxValue && c.AccumulatedMultiConstraintImpulse.SquareLength() > c.MaxImpulse * c.MaxImpulse)
            {
                c.AccumulatedMultiConstraintImpulse *= c.MaxImpulse / c.AccumulatedMultiConstraintImpulse.Length();
            }

            impulse = c.AccumulatedMultiConstraintImpulse - oldSum;

            ApplyImpulse(c, impulse);

            c.SaveImpulse();
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

        private static void ApplyImpulse(IConstraint c, Vector2D impulse)
        {
            if (c.B1 != null)
            {
                c.B1.Velocity -= impulse * c.B1.InverseMass;
                c.B1.AngularVelocity -= Vector2D.ZValueFromCross(c.R1, impulse) * c.B1.InverseInertia;
            }            

            if (c.B2 != null)
            {
                c.B2.Velocity += impulse * c.B2.InverseMass;
                c.B2.AngularVelocity += Vector2D.ZValueFromCross(c.R2, impulse) * c.B2.InverseInertia;
            }            
        }
    }
}
