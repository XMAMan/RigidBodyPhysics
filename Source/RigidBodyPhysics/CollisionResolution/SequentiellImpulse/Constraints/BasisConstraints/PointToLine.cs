using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.BasisConstraints
{
    //Point-To-Line-Constraint: Drückt Body2 auf die Achse von Body1
    internal class PointToLine : ILinear1DConstraint
    {
        public Vec2D R1 { get; } //Hebelarm vom B1.Center zum Ankerpunkt2 projetziert auf die B1.Center->Joint.Anchor1-Linie
        public Vec2D R2 { get; } //Hebelarm vom B2.Center zu Joint.Anchor2
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;

        public Vec2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        public float Bias { get; }
        public float ImpulseMass { get; } //Umrechungsvektor vom Relative-Kontaktpunktgeschwindigkeitswert in ein Impuls
        public float AccumulatedImpulse { get; set; }

        private IPointToLineJoint joint;

        public PointToLine(ConstraintConstructorData data, IPointToLineJoint joint)
        {
            this.joint = joint;
            var s = data.Settings;

            B1 = joint.B1;
            B2 = joint.B2;

            //d = x2+r2 - x1
            R1 = joint.Anchor2 - joint.B1.Center;        //R1=d=x1+r2-x1                 
            R2 = joint.Anchor2 - joint.B2.Center;        //R2=r2

            AccumulatedImpulse = s.DoWarmStart ? joint.AccumulatedPointToLineImpulse : 0;

            ForceDirection = joint.R1Dir.Spin90();

            Vec2D d = joint.B1ToA2;
            float s1 = Vec2D.ZValueFromCross(d, ForceDirection);
            float s2 = Vec2D.ZValueFromCross(R2, ForceDirection);

            float invMass = B1.InverseMass + s1 * s1 * B1.InverseInertia + B2.InverseMass + s2 * s2 * B2.InverseInertia;
            ImpulseMass = 1f / invMass;

            float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;
            this.Bias = -biasFactor * data.InvDt * (d * ForceDirection);
        }

        public void SaveImpulse()
        {
            joint.AccumulatedPointToLineImpulse = AccumulatedImpulse;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyWarmStartImpulse(this);
        }
        public void DoSingleSIStep()
        {
            ResolverHelper.DoSingleSIStepStiff(this);
        }
    }
}
