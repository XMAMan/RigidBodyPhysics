using GraphicMinimal;
using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Examples
{
    //Nutzt die Impulsformel (2.13) aus "3D Constraint Derivations for Impulse Solvers - Marijn 2015"

    //Simuliert die Bewegung einer Feder unter Nutzung der SoftConstraint-Formeln aus "3D Constraint Derivations for Impulse Solvers - Marijn 2015"
    //Option 1: Verwendung von Impulsformel (2.13)
    //  -> Feder mit Dämpfungsfaktor von 0 wird trotzdem gedämpft
    //  -> Hier darf ich kein Warmstart verwenden, da sonst die Federn mit jeden TimeStep immer mehr schwingen
    //Option 2: Verwendung von Impulsformel (2.17)
    //  -> Feder mit Dämpfungsfaktor von 0 wird trotzdem gedämpft
    //  -> Hier darf nun der Warmstart verwendet werden
    internal class ResolverHelper2
    {
        public enum Variation
        {
            Formula_2_13,
            Formula_2_17
        }

        public static void MoveBodiesWithConstraint(List<IRigidBody> bodies, List<IJoint> joints, float dt, SolverSettings settings, Variation variation)
        {
            //Schritt 1: Erzeuge ForceDirection, ImpulseMass und Bias für jedes Constraint-Objekt
            float invDt = dt > 0.0f ? 1.0f / dt : 0.0f;

            var constraints = joints.Select(joint => new DistanceJointConstraint(new ConstraintConstructorData()
            {
                Bodies = bodies,
                Joints = joints,
                Settings = settings,
                Dt = dt,
                InvDt = invDt
            }, (DistanceJoint)joint)).ToList();

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

                                        float impulse = float.NaN;
                    var d = (DistanceJointConstraint)c;
                    switch (variation)
                    {
                        case Variation.Formula_2_13:
                            //Impuls laut Formel (2.13) aus "3D Constraint Derivations for Impulse Solvers - Marijn 2015" -> Hier darf ich kein Warmstart verwenden
                            impulse = (-velocityInForceDirection - d.BetaCDeltaT) / d.K / settings.IterationCount;
                            break;

                        case Variation.Formula_2_17:
                            //(2.17)-Formel aus "3D Constraint Derivations for Impulse Solvers - Marijn 2015"
                            impulse = (-velocityInForceDirection - d.GammaDt * c.AccumulatedImpulse - d.BetaCDeltaT) / d.K;
                            break;
                    }
                    

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
