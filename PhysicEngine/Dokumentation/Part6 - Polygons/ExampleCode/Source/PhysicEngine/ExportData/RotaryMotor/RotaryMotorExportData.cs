namespace PhysicEngine.ExportData.RotaryMotor
{
    public class RotaryMotorExportData : IExportRotaryMotor
    {
        public int BodyIndex { get; set; }
        public float RotaryForce { get; set; }
        public bool IsEnabled { get; set; }
    }
}
