using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;
using PhysicEngine.RotaryMotor;
using PhysicEngine.Thruster;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse
{
    internal class ResolverHelper
    {
        internal static void MoveBodiesWithConstraint(List<IRigidBody> bodies, List<IJoint> joints, List<IThruster> thrusters, List<IRotaryMotor> motors, CollisionPointWithImpulse[] collisions, MouseConstraintData mouseData, float dt, SolverSettings settings)
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

            //Schritt 2: Wende die externe Kraft (Schwerkraft und Schubdüsen) für alle Körper an
            ApplyExternalForces(bodies, thrusters, motors, dt);

            //Es gibt keine Constraints(Keine Kollisionspunkte/Gelenke). Bewege die Körper ohne Beschränkung (Wende nur die externe Kraft an)
            if (constraints.Length == 0) return;

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

            //Schritt 5: Berechne, welche Zug- und Druck-Kräfte auf die Stäbe (längliche Rechtecke) gewirkt haben
            //           Wird nur für die Anzeige genutzt. Hat keine Auswirkung auf die Bewegung der Körper!
            foreach (var c in constraints)
            {
                if (c is ILinear1DConstraint)
                {
                    var cLin = (ILinear1DConstraint)c;
                    Vec2D impulse = cLin.ForceDirection * cLin.AccumulatedImpulse;
                    TrackBeamForce(c.B1, c.R1, -impulse);
                    TrackBeamForce(c.B2, c.R2, impulse);
                }else if (c is ILinearImpulse)
                {
                    var cLin = (ILinearImpulse)c;
                    Vec2D impulse = cLin.GetApplyedLinearImpulse();
                    TrackBeamForce(c.B1, c.R1, -impulse);
                    TrackBeamForce(c.B2, c.R2, impulse);
                }
            }
        }

        private static void ApplyExternalForces(List<IRigidBody> bodies, List<IThruster> thrusters, List<IRotaryMotor> motors, float dt)
        {
            //Gravity-Force
            foreach (var body in bodies)
            {
                body.Velocity.X += body.InverseMass * body.Force.X * dt;
                body.Velocity.Y += body.InverseMass * body.Force.Y * dt;
                body.AngularVelocity += body.InverseInertia * body.Torque * dt;

                //Wende Schwerkraft zu 50% aufs linke Stabende und zu 50% aufs rechte Stabende an
                if (body is IBeamForceTracker)
                {
                    var beam = (IBeamForceTracker)body;
                    beam.ResetTrackForce();
                    beam.AddTrackForce(body.Center, body.Force);
                }
            }

            //Thruster-Force
            foreach (var thruster in thrusters)
            {
                var body = thruster.B1;

                if (thruster.IsEnabled == false || body.InverseMass == 0)
                    continue;

                body.Velocity.X += body.InverseMass * thruster.Force.X * dt;
                body.Velocity.Y += body.InverseMass * thruster.Force.Y * dt;
                body.AngularVelocity += Vec2D.ZValueFromCross(thruster.R1, thruster.Force) * body.InverseInertia * dt;

                if (body is IBeamForceTracker)
                {
                    var beam = (IBeamForceTracker)body;
                    beam.AddTrackForce(thruster.Anchor, thruster.Force);
                }
            }

            //Rotary-Motor-Force
            foreach (var motor in motors)
            {
                var body = motor.B1;

                if (motor.IsEnabled == false || body.InverseMass == 0)
                    continue;


                body.AngularVelocity += motor.RotaryForce * body.InverseInertia * dt;
            }
        }

        private static void TrackBeamForce(IRigidBody body, Vec2D leverArm, Vec2D impulseDirection)
        {
            if (body is IBeamForceTracker)
            {
                var b = (IBeamForceTracker)body;
                b.AddTrackForce(body.Center + leverArm, impulseDirection);
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

        internal static void DoSingleSIStepStiff(I2DConstraint c)
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
            if (c.B1 != null)
            {
                c.B1.AngularVelocity -= angularImpulse * c.B1.InverseInertia;
            }

            if (c.B2 != null)
            {
                c.B2.AngularVelocity += angularImpulse * c.B2.InverseInertia;
            }
        }
    }
}
