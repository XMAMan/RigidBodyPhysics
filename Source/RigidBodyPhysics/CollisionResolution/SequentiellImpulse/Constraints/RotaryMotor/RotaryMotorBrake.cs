using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;

namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.RotaryMotor
{
    //Das ist die Bremse für ein RotaryMotor. Wenn aktiv, dann darf sich der Körper nicht mehr drehen
    internal class RotaryMotorBrake : IAngularConstraint
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

        private IRotaryMotor rotaryMotor;

        public RotaryMotorBrake(ConstraintConstructorData data, IRotaryMotor rotaryMotor)
        {
            this.rotaryMotor = rotaryMotor;
            var s = data.Settings;

            B1 = rotaryMotor.B1;

            this.AccumulatedImpulse = s.DoWarmStart ? rotaryMotor.AccumulatedBrakeImpulse : 0;
            this.ImpulseMass = 1f / B1.InverseInertia;

            float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;
            float positionError = B1.Angle - rotaryMotor.TargetAngluarValueForBrake;
            Bias = -biasFactor * data.InvDt * positionError;

        }

        public void SaveImpulse()
        {
            rotaryMotor.AccumulatedBrakeImpulse = AccumulatedImpulse;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyAngularImpulse(this, rotaryMotor.AccumulatedBrakeImpulse);
        }
        public void DoSingleSIStep()
        {
            float angularVelocity = -this.B1.AngularVelocity;

            float angularImpulse = this.ImpulseMass * (this.Bias - angularVelocity);

            ResolverHelper.ApplyAngularImpulse(this, angularImpulse);

            this.SaveImpulse();
        }
    }
}
