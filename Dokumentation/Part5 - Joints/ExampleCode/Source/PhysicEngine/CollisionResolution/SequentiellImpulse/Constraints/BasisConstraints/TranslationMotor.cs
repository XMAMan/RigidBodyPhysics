using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;
using static PhysicEngine.Joints.IPublicJoint;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.BasisConstraints
{
    //Läßt Body2 durch ein Motor zu einer angegebenen Position fahren oder mit einer bestimmten Geschwindigkeit sich bewegen
    internal class TranslationMotor : ISoftConstraint1D
    {
        public Vec2D R1 { get; } //Hebelarm vom B1.Center zum Ankerpunkt2 projetziert auf die B1.Center->Joint.Anchor1-Linie
        public Vec2D R2 { get; } //Hebelarm vom B2.Center zu Joint.Anchor2
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;

        public bool IsSoftConstraint { get; } = false;
        public float Gamma { get; private set; } = 0;
        public float Beta { get; private set; } = 0;
        public float PositionError { get; }


        public Vec2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        public float Bias { get; }        
        public float ImpulseMass { get; private set; } = 0; //Umrechungsvektor vom Relative-Kontaktpunktgeschwindigkeitswert in ein Impuls
        public float AccumulatedImpulse { get; set; }

        private ITranslationMotorJoint joint;

        public TranslationMotor(ConstraintConstructorData data, ITranslationMotorJoint joint)
        {
            this.joint = joint;
            var s = data.Settings;

            Vec2D d = joint.B1ToA2;

            B1 = joint.B1;
            B2 = joint.B2;
            R1 = d;
            R2 = joint.Anchor2 - joint.B2.Center;

            AccumulatedImpulse = s.DoWarmStart ? joint.AccumulatedTranslationMotorImpulse : 0;

            this.ForceDirection = joint.R1Dir;
            float s1 = Vec2D.ZValueFromCross(d, ForceDirection);
            float s2 = Vec2D.ZValueFromCross(R2, ForceDirection);

            float invMass = B1.InverseMass + s1 * s1 * B1.InverseInertia + B2.InverseMass + s2 * s2 * B2.InverseInertia;
            this.ImpulseMass = 1.0f / invMass;


            float maxImpulse = data.Dt * joint.MaxMotorForce;
            this.MinImpulse = -maxImpulse;
            this.MaxImpulse = maxImpulse;
            float currentDistance = ForceDirection * d;

            if (joint.Motor == IPublicPrismaticJoint.TranslationMotor.GoToReferencePosition)
            {
                float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;
                float pos = joint.MotorPixelPosition;
                this.PositionError = currentDistance - pos;
                Bias = -biasFactor * data.InvDt * this.PositionError;

                if (joint.Soft.ParameterType != SpringParameter.NoSoftness)
                {
                    IsSoftConstraint = true;
                    joint.Soft.GetSoftConstraintParameters(data.Dt, invMass, x => Gamma = x, x => Beta = x, x => ImpulseMass = x);
                }
            }

            if (joint.Motor == IPublicPrismaticJoint.TranslationMotor.IsMoving)
            {
                float min = joint.DistanceOnStart * joint.MinTranslation; //Umrechung der Min-Max-Werte in Pixel
                float max = joint.DistanceOnStart * joint.MaxTranslation;

                //Bewege nur dann den Motor, wenn er innerhalb der MinMax-Limits liegt
                if (joint.LimitIsEnabled == false || joint.LimitIsEnabled && (joint.MotorSpeed < 0 && currentDistance > min || (joint.MotorSpeed > 0 && currentDistance < max)))
                {
                    Bias = joint.MotorSpeed;
                }
                else
                {
                    Bias = 0;
                }
            }

            
        }

        public void SaveImpulse()
        {
            joint.AccumulatedTranslationMotorImpulse = AccumulatedImpulse;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyWarmStartImpulse(this);
        }
        public void DoSingleSIStep()
        {
            ResolverHelper.DoSingleSIStepSoft(this);
        }
    }
}
