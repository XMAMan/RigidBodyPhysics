﻿namespace PhysicEngine.ExportData.RotaryMotor
{
    public interface IExportRotaryMotor
    {
        int BodyIndex { get; set; }
        float RotaryForce { get; set; }
        bool IsEnabled { get; set; }
    }
}