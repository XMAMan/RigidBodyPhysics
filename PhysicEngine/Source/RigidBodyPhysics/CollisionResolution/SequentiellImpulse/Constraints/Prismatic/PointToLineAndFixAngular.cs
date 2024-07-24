using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.Prismatic
{
    internal class PointToLineAndFixAngular : I2DConstraint, ILinearImpulse
    {
        public Vec2D R1 { get; } //Hebelarm vom B1.Center zum Anchorpoint1
        public Vec2D R2 { get; } //Hebelarm vom B2.Center zum Anchorpoint2
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;


        public Matrix2x2 InverseK { get; } = null;    //Wenn IsMultiConstraint=true, dann muss hier ein Wert stehen. K=J*M^-1*J^T
        public Vec2D GetCDot() //Gibt CDot=J*V zurück (Geschwindigkeit in Richtung jeder J-Zeile)
        {
            float pointToLineVelocity = -t1 * B1.Velocity - Vec2D.ZValueFromCross(B2.Center + R2 - B1.Center, t1) * B1.AngularVelocity + t1 * B2.Velocity + Vec2D.ZValueFromCross(R2, t1) * B2.AngularVelocity; ;
            float angularVelocity = B2.AngularVelocity - B1.AngularVelocity;

            return new Vec2D(pointToLineVelocity, angularVelocity);
        }
        public Vec2D Bias { get; }
        public Vec2D AccumulatedImpulse { get; set; } = null;


        private Vec2D t1; //PointToLine-Forcedirection

        private PrismaticJoint joint;

        public PointToLineAndFixAngular(ConstraintConstructorData data, PrismaticJoint joint)
        {
            this.joint = joint;

            B1 = joint.B1;
            B2 = joint.B2;

            //R1/R2 wird für die ResolverHelper.ApplyLinearImpulse benötigt. Deswegen setze ich hier die Werte wie bei PointToLine
            R1 = joint.Anchor2 - joint.B1.Center;
            R2 = joint.Anchor2 - joint.B2.Center;

            this.t1 = (joint.Anchor1 - B1.Center).Spin90().Normalize();
            Vec2D d = joint.Anchor2 - B1.Center;

            this.AccumulatedImpulse = data.Settings.DoWarmStart ? new Vec2D(joint.AccumulatedPointToLineImpulse, joint.AccumulatedAngularImpulse) : new Vec2D(0, 0);

            float pointToLinePositionError = d * this.t1;
            float angularPositionError = B2.Angle - B1.Angle - joint.AngularDifferenceOnStart;

            Vec2D positionError = new Vec2D(pointToLinePositionError, angularPositionError);
            this.Bias = data.Settings.PositionalCorrectionRate * data.InvDt * positionError;

            float s1 = -Vec2D.ZValueFromCross(d, this.t1);
            float s2 = Vec2D.ZValueFromCross(R2, this.t1);

            float k2 = -s1 * B1.InverseInertia + s2 * B2.InverseInertia;
            float k4 = B1.InverseInertia + B2.InverseInertia;
            if (k4 == 0) k4 = 1;// For bodies with fixed rotation. //Quelle: https://github.com/erincatto/box2d/blob/main/src/dynamics/b2_prismatic_joint.cpp#L170C4-L170C38

            this.InverseK = Matrix2x2.FromScalars(
                    B1.InverseMass + B2.InverseMass + s1 * s1 * B1.InverseInertia + s2 * s2 * B2.InverseInertia,
                    k2,
                    k2,
                    k4
                ).Invert();
        }

        public void SaveImpulse()
        {
            joint.AccumulatedPointToLineImpulse = this.AccumulatedImpulse.X;
            joint.AccumulatedAngularImpulse = this.AccumulatedImpulse.Y;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyLinearImpulse(this, this.t1 * joint.AccumulatedPointToLineImpulse);
            ResolverHelper.ApplyAngularImpulse(this, joint.AccumulatedAngularImpulse);
        }
        public void DoSingleSIStep()
        {
            Vec2D impulse = this.InverseK * (-this.Bias - this.GetCDot());

            AccumulatedImpulse += impulse;

            ResolverHelper.ApplyLinearImpulse(this, this.t1 * impulse.X);
            ResolverHelper.ApplyAngularImpulse(this, impulse.Y);

            SaveImpulse();
        }

        public Vec2D GetApplyedLinearImpulse()
        {
            return this.t1 * joint.AccumulatedPointToLineImpulse;
        }
    }
}
