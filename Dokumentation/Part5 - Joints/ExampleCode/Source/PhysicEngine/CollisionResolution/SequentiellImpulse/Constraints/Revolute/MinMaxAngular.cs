using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.Revolute
{
    //Sorgt dafür, dass sich Body2 gegenüber Body1 nur innerhalb der Min-Max-Schranke drehen kann
    internal class MinMaxAngular : IAngularConstraint
    {
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public Vec2D R1 { get; } //Hebelarm vom B1.Center zu Joint.Anchor1
        public Vec2D R2 { get; } //Hebelarm vom B2.Center zu Joint.Anchor2
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;

        public float Bias { get; }
        public float ImpulseMass { get; } //Umrechungsvektor vom Relative-Kontaktpunktgeschwindigkeitswert in ein Impuls
        public float AccumulatedImpulse { get; set; }


        private IMinMaxAngularJoint joint;

        public MinMaxAngular(ConstraintConstructorData data, IMinMaxAngularJoint joint)
        {
            this.joint = joint;
            var s = data.Settings;

            B1 = joint.B1;
            B2 = joint.B2;
            R1 = joint.Anchor1 - joint.B1.Center;
            R2 = joint.Anchor2 - joint.B2.Center;

            AccumulatedImpulse = s.DoWarmStart ? joint.AccumulatedMinMaxAngularImpulse : 0;

            float impulseMass = 1f / (B1.InverseInertia + B2.InverseInertia);

            float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;

            float angle = B2.Angle - B1.Angle - joint.AngularDifferenceOnStart + joint.DiffToMinOnStart;
            if (angle < 0)
            {
                this.ImpulseMass = impulseMass;
                MinImpulse = 0; //Drehe nur so, dass angle erhöht wird
                Bias = -biasFactor * data.InvDt * angle;
            }
            if (angle > joint.MinMaxDifference)
            {
                this.ImpulseMass = impulseMass;
                MaxImpulse = 0;  //Drehe nur so, dass angle verringert wird
                Bias = -biasFactor * data.InvDt * (angle - joint.MinMaxDifference);
            }
        }

        public void SaveImpulse()
        {
            joint.AccumulatedMinMaxAngularImpulse = AccumulatedImpulse;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyAngularImpulse(this, joint.AccumulatedMinMaxAngularImpulse);
        }
        public void DoSingleSIStep()
        {
            ResolverHelper.DoSingleSIStepForAngularImpulseStiff(this);
        }
    }
}
