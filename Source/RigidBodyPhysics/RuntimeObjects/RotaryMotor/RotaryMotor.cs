using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints;
using RigidBodyPhysics.ExportData.RotaryMotor;
using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.RotaryMotor
{
    internal class RotaryMotor : IRotaryMotor
    {
        #region IPublicRotaryMotor
        public IPublicRigidBody Body { get; }
        public float RotaryForce { get; set; }
        public float MaxSpeed { get; set; }
        private bool isEnabled = false;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    IsEnabledChanged?.Invoke(value);
                }
            }
        }
        public event Action<bool> IsEnabledChanged;

        private bool breakIsEnabled = false;
        public bool BrakeIsEnabled
        {
            get => breakIsEnabled;
            set
            {
                if (breakIsEnabled != value)
                {
                    breakIsEnabled = value;
                    TargetAngluarValueForBrake = B1.Angle;
                }
            }
        }
        #endregion

        #region IRotaryMotor
        public IRigidBody B1 { get; }
        public float AccumulatedBrakeImpulse { get; set; } = 0; //Aufsummierte Impulse von der Bremse
        public float TargetAngluarValueForBrake { get; private set; } = float.NaN; //Diesen Angle-Wert hatte der Körper, als BreakIsEnabled auf true ging
        #endregion

        public IExportRotaryMotor GetExportData(List<IRigidBody> bodies)
        {
            return new RotaryMotorExportData()
            {
                BodyIndex = bodies.IndexOf(B1),
                RotaryForce = RotaryForce,
                MaxSpeed = MaxSpeed,
                IsEnabled = IsEnabled,
                BrakeIsEnabled = BrakeIsEnabled,
            };
        }

        public RotaryMotor(RotaryMotorExportData data, List<IRigidBody> bodies)
        {
            if (data.MaxSpeed < 0) throw new ArgumentException("MaxSpeed must be positive");

            Body = B1 = bodies[data.BodyIndex];
            RotaryForce = data.RotaryForce;
            MaxSpeed = data.MaxSpeed;
            IsEnabled = data.IsEnabled;
            BrakeIsEnabled = data.BrakeIsEnabled;
        }

        public List<IConstraint> BuildConstraints(ConstraintConstructorData data)
        {
            List<IConstraint> list = new List<IConstraint>();

            if (BrakeIsEnabled)
                list.Add(new RotaryMotorBrake(data, this));

            return list;
        }
    }
}
