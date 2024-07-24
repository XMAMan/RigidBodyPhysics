namespace RigidBodyPhysics.ExportData.RotaryMotor
{
    public class RotaryMotorExportData : IExportRotaryMotor
    {
        public int BodyIndex { get; set; }
        public float RotaryForce { get; set; }
        public float MaxSpeed { get; set; }
        public bool IsEnabled { get; set; }
        public bool BrakeIsEnabled { get; set; }

        public RotaryMotorExportData() { }

        public RotaryMotorExportData(RotaryMotorExportData copy)
        {
            this.BodyIndex = copy.BodyIndex;
            this.RotaryForce = copy.RotaryForce;
            this.MaxSpeed = copy.MaxSpeed;
            this.IsEnabled = copy.IsEnabled;
            this.BrakeIsEnabled = copy.BrakeIsEnabled;
        }

        public IExportRotaryMotor GetCopy()
        {
            return new RotaryMotorExportData(this);
        }
    }
}
