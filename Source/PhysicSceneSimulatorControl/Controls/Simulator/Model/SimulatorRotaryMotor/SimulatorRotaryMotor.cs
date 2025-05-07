using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using PhysicGlobal;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorRotaryMotor
{
    internal class SimulatorRotaryMotor : ISimulatorRotaryMotor
    {
        public IPublicRotaryMotor PhysicModel { get; }
        public SimulatorRotaryMotor(IPublicRotaryMotor ctor)
        {
            this.PhysicModel = ctor;
        }
        public void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings)
        {
            var c = this.PhysicModel;

            if (printSettings.ShowRotaryMotors)
            {
                DrawArrow(panel, c.IsEnabled ? Pens.Red : Pens.Blue);
            }
        }

        private void DrawArrow(IDrawingPanel panel, Pen pen)
        {
            var c = this.PhysicModel;

            var center = this.PhysicModel.Body.Center;
            panel.DrawCircleArc(pen, center, 20, 30, 320, false);
            var p = Vec2D.RotatePointAroundPivotPoint(center, center + new Vec2D(20, 0), 320);
            var dir1 = Vec2D.RotatePointAroundPivotPoint(center, center + new Vec2D(20 + 10, 0 - 10), 320);
            var dir2 = Vec2D.RotatePointAroundPivotPoint(center, center + new Vec2D(20 - 10, 0 - 10), 320);

            panel.DrawLine(pen, p, dir1);
            panel.DrawLine(pen, p, dir2);
        }
    }
}
