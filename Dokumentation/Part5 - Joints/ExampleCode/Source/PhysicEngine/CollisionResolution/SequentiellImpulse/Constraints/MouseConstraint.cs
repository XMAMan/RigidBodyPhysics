using PhysicEngine.MathHelper;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    //Körper, der per Maus angeklickt wurde und festegehalten wird
    internal class MouseConstraint : ISoftConstraint2D
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
            var b = this.mouseData.RigidBody;
            Vec2D v = b.Velocity + new Vec2D(-b.AngularVelocity * this.R2.Y, b.AngularVelocity * this.R2.X);
            return v;
        }        
        public Vec2D Bias { get; }
        public Vec2D AccumulatedImpulse { get; set; } = new Vec2D(0, 0);

        

        private MouseConstraintData mouseData;

        public MouseConstraint(ConstraintConstructorData data, MouseConstraintData mouseData)
        {
            this.mouseData = mouseData;

            this.B2 = mouseData.RigidBody;
            this.B1 = null;
            this.R2 = MathHelp.GetWorldDirectionFromLocalDirection(mouseData.RigidBody, mouseData.LocalAnchorDirection);
            this.R1 = null;

            this.AccumulatedImpulse = data.Settings.DoWarmStart ? mouseData.AccumulatedImpulse : new Vec2D(0, 0);

            var b = mouseData.RigidBody;
            var r = this.R2;

            this.PositionError = b.Center + r - this.mouseData.MousePosition;
            this.Bias = data.InvDt * PositionError;

            this.MaxImpulse = data.Dt * mouseData.MaxForce;

            if (mouseData.Soft.ParameterType != Joints.IPublicJoint.SpringParameter.NoSoftness)
            {
                this.IsSoftConstraint = true;
                mouseData.Soft.GetSoftConstraintParameters(data.Dt, float.NaN, x => Gamma = x, x => Beta = x, x => { });
            }            

            this.InverseK = Matrix2x2.FromScalars(
                    b.InverseMass + r.Y * r.Y * b.InverseInertia + this.Gamma,
                    -r.X * r.Y * b.InverseInertia,
                    -r.X * r.Y * b.InverseInertia,
                    b.InverseMass + r.X * r.X * b.InverseInertia + this.Gamma
                )
                .Invert();
        }

        public void SaveImpulse()
        {
            mouseData.AccumulatedImpulse = this.AccumulatedImpulse;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyLinearImpulse(this, this.AccumulatedImpulse);
        }
        public void DoSingleSIStep()
        {
            ResolverHelper.DoSingleSIStepSoft(this); 
        }
    }
}
