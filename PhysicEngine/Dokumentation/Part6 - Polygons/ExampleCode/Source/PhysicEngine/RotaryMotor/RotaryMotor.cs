using PhysicEngine.ExportData.RotaryMotor;
using PhysicEngine.RigidBody;

namespace PhysicEngine.RotaryMotor
{
    internal class RotaryMotor : IRotaryMotor
    {
        #region IPublicRotaryMotor
        public IPublicRigidBody Body { get; }
        public float RotaryForce { get; set; }
        public bool IsEnabled { get; set; }
        #endregion

        #region IRotaryMotor
        public IRigidBody B1 { get; }
        #endregion

        public IExportRotaryMotor GetExportData(List<IRigidBody> bodies)
        {
            return new RotaryMotorExportData()
            {
                BodyIndex = bodies.IndexOf(B1),
                RotaryForce = this.RotaryForce,
                IsEnabled = this.IsEnabled
            };
        }

        public RotaryMotor(RotaryMotorExportData data, List<IRigidBody> bodies)
        {
            this.Body = this.B1 = bodies[data.BodyIndex];
            this.RotaryForce = data.RotaryForce;
            this.IsEnabled = data.IsEnabled;
        }
    }
}
