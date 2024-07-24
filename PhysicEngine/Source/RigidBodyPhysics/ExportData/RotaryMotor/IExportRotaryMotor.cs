namespace RigidBodyPhysics.ExportData.RotaryMotor
{
    public interface IExportRotaryMotor
    {
        int BodyIndex { get; set; }
        float RotaryForce { get; set; }
        bool IsEnabled { get; set; }
        bool BrakeIsEnabled { get; set; }

        IExportRotaryMotor GetCopy();
    }
}
