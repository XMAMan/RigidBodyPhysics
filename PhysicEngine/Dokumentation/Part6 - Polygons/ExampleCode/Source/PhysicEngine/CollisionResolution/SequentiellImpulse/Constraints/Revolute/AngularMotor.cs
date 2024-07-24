using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;
using static PhysicEngine.Joints.IPublicJoint;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.Revolute
{
    //Sorgt dafür, dass sich Body2 gegenüber Body1 nur innerhalb der Min-Max-Schranke drehen kann
    internal class AngularMotor : ISoftAngular
    {
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public Vec2D R1 { get; } //Hebelarm vom B1.Center zu Joint.Anchor1
        public Vec2D R2 { get; } //Hebelarm vom B2.Center zu Joint.Anchor2
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;

        public bool IsSoftConstraint { get; } = false;
        public float Gamma { get; private set; } = 0;
        public float Beta { get; private set; } = 0;
        public float PositionError { get; } = 0;

        public float Bias { get; }        
        public float ImpulseMass { get; private set; } //Umrechungsvektor vom Relative-Kontaktpunktgeschwindigkeitswert in ein Impuls
        public float AccumulatedImpulse { get; set; }

        private IAngularMotorJoint joint;

        public AngularMotor(ConstraintConstructorData data, IAngularMotorJoint joint)
        {
            this.joint = joint;
            var s = data.Settings;

            B1 = joint.B1;
            B2 = joint.B2;
            R1 = joint.Anchor1 - joint.B1.Center;
            R2 = joint.Anchor2 - joint.B2.Center;

            AccumulatedImpulse = s.DoWarmStart ? joint.AccumulatedAngularMotorImpulse : 0;

            float invMass = B1.InverseInertia + B2.InverseInertia;
            this.ImpulseMass = 1f / invMass;

            float angle = B2.Angle - B1.Angle - joint.AngularDifferenceOnStart + joint.DiffToMinOnStart;//angle=0..joint.MinMaxDifference

            float maxImpulse = data.Dt * joint.MaxMotorTorque;
            this.MinImpulse = -maxImpulse;
            this.MaxImpulse = maxImpulse;

            if (joint.Motor ==  IPublicJoint.AngularMotor.GoToReferenceAngle)
            {
                float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;

                float setAngle = joint.MotorPosition * joint.MinMaxDifference;

                this.PositionError = angle - setAngle;
                Bias = -biasFactor * data.InvDt * this.PositionError;

                if (joint.Soft.ParameterType != SpringParameter.NoSoftness)
                {
                    IsSoftConstraint = true;
                    joint.Soft.GetSoftConstraintParameters(data.Dt, invMass, x => Gamma = x, x => Beta = x, x => ImpulseMass = x);
                }
            }

            if (joint.Motor == IPublicJoint.AngularMotor.SpinAround)
            {
                //Drehe nur dann den Motor, wenn er innerhalb der MinMax-Limits liegt
                if (joint.LimitIsEnabled == false || joint.LimitIsEnabled && (joint.MotorSpeed < 0 && angle > 0 || (joint.MotorSpeed > 0 && angle < joint.MinMaxDifference)))
                {
                    Bias = joint.MotorSpeed;
                }else
                {
                    Bias = 0;
                }
            }
        }

        public void SaveImpulse()
        {
            joint.AccumulatedAngularMotorImpulse = AccumulatedImpulse;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyAngularImpulse(this, joint.AccumulatedAngularMotorImpulse);
        }
        public void DoSingleSIStep()
        {
            ResolverHelper.DoSingleSIStepForAngularImpulseSoft(this);
        }
    }
}
