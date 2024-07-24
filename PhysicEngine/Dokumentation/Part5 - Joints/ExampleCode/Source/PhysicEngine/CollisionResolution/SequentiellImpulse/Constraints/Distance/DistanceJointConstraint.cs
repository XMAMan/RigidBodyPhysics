using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;
using static PhysicEngine.Joints.IPublicJoint;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.Distance
{
    //Stange oder Eisenfeder ohne Min-Max-Schranke
    internal class DistanceJointConstraint : ISoftConstraint1D
    {
        public Vec2D R1 { get; } //Hebelarm vom B1.Center zum Anchorpoint1
        public Vec2D R2 { get; } //Hebelarm vom B2.Center zum Anchorpoint2
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;

        public bool IsSoftConstraint { get; }
        public float Gamma { get; private set; } = 0;
        public float Beta { get; private set; } = 0;
        public float PositionError { get; }

        public Vec2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        public float Bias { get; }        
        public float ImpulseMass { get; private set; } //Umrechungsvektor vom Relative-Ankerpunktgeschwindigkeitswert in ein Impuls
        public float AccumulatedImpulse { get; set; }

        private DistanceJoint joint;
        public DistanceJointConstraint(ConstraintConstructorData data, DistanceJoint joint)
        {
            this.joint = joint;
            var s = data.Settings;

            B1 = joint.B1;
            B2 = joint.B2;
            R1 = joint.Anchor1 - joint.B1.Center;
            R2 = joint.Anchor2 - joint.B2.Center;

            AccumulatedImpulse = s.DoWarmStart ? joint.AccumulatedImpulse : 0;

            Vec2D a1Toa2 = joint.Anchor2 - joint.Anchor1;
            float length = a1Toa2.Length();
            Vec2D n = length > 0.0001f ? a1Toa2 / length : new Vec2D(1, 0);

            this.PositionError = length - joint.Length;

            ForceDirection = n;

            float r1crossN = Vec2D.ZValueFromCross(R1, n);
            float r2crossN = Vec2D.ZValueFromCross(R2, n);

            float invMass = B1.InverseMass + B1.InverseInertia * r1crossN * r1crossN + B2.InverseMass + B2.InverseInertia * r2crossN * r2crossN;
            ImpulseMass = 1f / invMass;

            float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;
            Bias = -biasFactor * data.InvDt * this.PositionError;

            if (joint.Soft.ParameterType != SpringParameter.NoSoftness)
            {
                IsSoftConstraint = true;
                joint.Soft.GetSoftConstraintParameters(data.Dt, invMass, x => Gamma = x, x => Beta = x, x => ImpulseMass = x);
            }


        }

        public void SaveImpulse()
        {
            joint.AccumulatedImpulse = AccumulatedImpulse;
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
