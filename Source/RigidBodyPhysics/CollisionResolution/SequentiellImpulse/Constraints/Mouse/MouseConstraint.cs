using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.MouseBodyClick;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.Mouse
{
    //Körper, der per Maus angeklickt wurde und festegehalten wird
    internal class MouseConstraint : ISoftConstraint2D, ILinearImpulse
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
        public Vec2D PositionError { get; }

        public Matrix2x2 InverseK { get; } = null;    //Wenn IsMultiConstraint=true, dann muss hier ein Wert stehen. K=J*M^-1*J^T
        public Vec2D GetCDot() //Gibt CDot=J*V zurück (Geschwindigkeit in Richtung jeder J-Zeile)
        {
            var b = mouseData.RigidBody;
            Vec2D v = b.Velocity + new Vec2D(-b.AngularVelocity * R2.Y, b.AngularVelocity * R2.X);
            return v;
        }
        public Vec2D Bias { get; }
        public Vec2D AccumulatedImpulse { get; set; } = new Vec2D(0, 0);



        private MouseConstraintData mouseData;

        public MouseConstraint(ConstraintConstructorData data, MouseConstraintData mouseData)
        {
            this.mouseData = mouseData;

            B2 = mouseData.RigidBody;
            B1 = null;
            R2 = MathHelp.GetWorldDirectionFromLocalDirection(mouseData.RigidBody, mouseData.LocalAnchorDirection);
            R1 = null;

            AccumulatedImpulse = data.Settings.DoWarmStart ? mouseData.AccumulatedImpulse : new Vec2D(0, 0);

            var b = mouseData.RigidBody;
            var r = R2;

            PositionError = b.Center + r - this.mouseData.MousePosition;
            Bias = data.InvDt * PositionError;

            MaxImpulse = data.Dt * mouseData.MaxForce;

            if (mouseData.Soft.ParameterType != IPublicJoint.SpringParameter.NoSoftness)
            {
                IsSoftConstraint = true;
                mouseData.Soft.GetSoftConstraintParameters(data.Dt, float.NaN, x => Gamma = x, x => Beta = x, x => { });
            }

            InverseK = Matrix2x2.FromScalars(
                    b.InverseMass + r.Y * r.Y * b.InverseInertia + Gamma,
                    -r.X * r.Y * b.InverseInertia,
                    -r.X * r.Y * b.InverseInertia,
                    b.InverseMass + r.X * r.X * b.InverseInertia + Gamma
                )
                .Invert();
        }

        public void SaveImpulse()
        {
            mouseData.AccumulatedImpulse = AccumulatedImpulse;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyLinearImpulse(this, AccumulatedImpulse);
        }
        public void DoSingleSIStep()
        {
            ResolverHelper.DoSingleSIStepSoft(this);
        }

        public Vec2D GetApplyedLinearImpulse()
        {
            return AccumulatedImpulse;
        }
    }
}
