using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;
using static PhysicEngine.Joints.IPublicJoint;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.Weld
{
    //Diese Klasse ist ein Beispiel, wo man eine Constraint implementiert, welche aus 3 J-Zeilen besteht
    internal class PointToPointAndFixAngular : ISoftConstraint3D, ILinearImpulse
    {
        //Schritt 1: Erzeuge über den Konstruktor folgende Propertys
        public Vec2D R1 { get; } //Hebelarm vom B1.Center zum Anchorpoint1
        public Vec2D R2 { get; } //Hebelarm vom B2.Center zum Anchorpoint2
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;

        public bool IsSoftConstraint { get; } = false;
        public float Gamma { get; private set; } = 0;
        public float Beta { get; private set; } = 0;
        public Vec3D PositionError { get; }

        private IPointToPointAndFixAngularJoint joint;
        private Matrix3x3 inverseK;
        private Vec3D bias; //X/Y = Point2Point; Z=FixAngular
        private Vec3D accumulatedImpulse;

        public PointToPointAndFixAngular(ConstraintConstructorData data, IPointToPointAndFixAngularJoint joint)
        {
            this.joint = joint;

            this.B1 = joint.B1;
            this.B2 = joint.B2;
            this.R1 = joint.Anchor1 - joint.B1.Center;
            this.R2 = joint.Anchor2 - joint.B2.Center;

            var s = data.Settings;
            this.accumulatedImpulse = s.DoWarmStart ? new Vec3D(joint.AccumulatedPointToPointImpulse.X, joint.AccumulatedPointToPointImpulse.Y, joint.AccumulatedAngularImpulse) : new Vec3D(0, 0, 0);

            this.PositionError = new Vec3D(
                joint.Anchor2.X - joint.Anchor1.X,
                joint.Anchor2.Y - joint.Anchor1.Y,
                B2.Angle - B1.Angle - joint.AngularDifferenceOnStart);

            //this.bias = s.PositionalCorrectionRate * data.InvDt * PositionError;
            this.bias = new Vec3D(PositionError.X, PositionError.Y, s.PositionalCorrectionRate * PositionError.Z) * data.InvDt; //Die PositionalCorrectionRate für Point2Point ist 1

            float m1 = B1.InverseMass, m2 = B2.InverseMass, I1 = B1.InverseInertia, I2 = B2.InverseInertia;

            float k2 = -R1.X * R1.Y * I1 - R2.X * R2.Y * I2;
            float k3 = -R1.Y * I1 - R2.Y * I2;
            float k6 = R1.X * I1 + R2.X * I2;

            if (joint.Soft.ParameterType != SpringParameter.NoSoftness)
            {
                IsSoftConstraint = true;
                joint.Soft.GetSoftConstraintParameters(data.Dt, float.NaN, x => Gamma = x, x => Beta = x, x => { }) ;
            }

            this.inverseK = new Matrix3x3(new float[] {
                    m1 + R1.Y*R1.Y*I1 + m2 + R2.Y*R2.Y*I2 + this.Gamma,
                    k2,
                    k3,
                    k2,
                    m1 + R1.X* R1.X*I1 + m2 + R2.X* R2.X*I2 + this.Gamma,
                    k6,
                    k3,
                    k6,
                    I1 + I2 + this.Gamma
            })
                .Invert();            
        }
        public void SaveImpulse()
        {
            joint.AccumulatedPointToPointImpulse = accumulatedImpulse.XY;
            joint.AccumulatedAngularImpulse = this.accumulatedImpulse.Z;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyLinearImpulse(this, this.accumulatedImpulse.XY);
            ResolverHelper.ApplyAngularImpulse(this, this.accumulatedImpulse.Z);
        }
        public void DoSingleSIStep()
        {
            Vec3D impulse = null;
            if (IsSoftConstraint == false)
            {
                impulse = inverseK * (-bias - GetCDot());
            }
            else
            {
                impulse = inverseK * (-GetCDot() - Gamma * accumulatedImpulse - Beta * this.PositionError);
            }

            this.accumulatedImpulse += impulse;

            ResolverHelper.ApplyLinearImpulse(this, impulse.XY);
            ResolverHelper.ApplyAngularImpulse(this, impulse.Z);

            this.SaveImpulse();
        }

        private Vec3D GetCDot() //Gibt CDot=J*V zurück (Geschwindigkeit in Richtung jeder J-Zeile)
        {
            float cDotX = -B1.Velocity.X + R1.Y*B1.AngularVelocity + B2.Velocity.X - R2.Y*B2.AngularVelocity;
            float cDotY = -B1.Velocity.Y - R1.X*B1.AngularVelocity + B2.Velocity.Y + R2.X*B2.AngularVelocity;
            float cDotZ = -B1.AngularVelocity + B2.AngularVelocity;

            return new Vec3D(cDotX, cDotY, cDotZ);
        }

        public Vec2D GetApplyedLinearImpulse()
        {
            return this.accumulatedImpulse.XY;
        }
    }
}
