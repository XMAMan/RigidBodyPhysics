using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;
using static PhysicEngine.Joints.IPublicJoint;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.BasisConstraints
{
    //Sorgt dafür, dass sich Body2 gegenüber Body1 nicht mehr drehen kann
    internal class FixAngular : ISoftAngular
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
        public float PositionError { get; }

        public float Bias { get; }        
        public float ImpulseMass { get; private set; } //Umrechungsvektor vom Relative-Kontaktpunktgeschwindigkeitswert in ein Impuls
        public float AccumulatedImpulse { get; set; }


        private IFixAngularJoint joint;

        public FixAngular(ConstraintConstructorData data, IFixAngularJoint joint, bool useSoftOption)
        {
            this.joint = joint;
            var s = data.Settings;

            B1 = joint.B1;
            B2 = joint.B2;
            R1 = joint.Anchor1 - joint.B1.Center;
            R2 = joint.Anchor2 - joint.B2.Center;

            AccumulatedImpulse = s.DoWarmStart ? joint.AccumulatedAngularImpulse : 0;

            float invMass = B1.InverseInertia + B2.InverseInertia;
            ImpulseMass = 1f / invMass;

            this.PositionError = B2.Angle - B1.Angle - joint.AngularDifferenceOnStart;

            float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;
            Bias = -biasFactor * data.InvDt * this.PositionError;

            if (useSoftOption && joint.Soft.ParameterType != SpringParameter.NoSoftness)
            {
                IsSoftConstraint = true;
                joint.Soft.GetSoftConstraintParameters(data.Dt, invMass, x => Gamma = x, x => Beta = x, x => ImpulseMass = x);
            }
        }

        public void SaveImpulse()
        {
            joint.AccumulatedAngularImpulse = AccumulatedImpulse;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyAngularImpulse(this, joint.AccumulatedAngularImpulse);
        }
        public void DoSingleSIStep()
        {
            ResolverHelper.DoSingleSIStepForAngularImpulseSoft(this);
        }
    }
}
