using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using PhysicGlobal;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorThruster
{
    internal class SimulatorThruster : ISimulatorThruster
    {
        public IPublicThruster PhysicModel { get; }
        public SimulatorThruster(IPublicThruster ctor)
        {
            this.PhysicModel = ctor;
        }
        public void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings)
        {
            var c = this.PhysicModel;

            if (printSettings.ShowThrusters)
            {
                DrawArrow(panel, c.IsEnabled ? Pens.Red : Pens.Blue);
            }
        }

        private void DrawArrow(IDrawingPanel panel, Pen pen)
        {
            var c = this.PhysicModel;

            float r = 50;
            Vec2D forceDirection = c.ForceDirection;
            var v1 = Vec2D.GetV2FromAngle360(forceDirection, 45 + 90);
            var v2 = Vec2D.GetV2FromAngle360(forceDirection, -45 - 90);

            panel.DrawLine(pen, (c.Anchor - forceDirection * r), c.Anchor);
            panel.DrawLine(pen, c.Anchor, (c.Anchor + v1 * (r / 3)));
            panel.DrawLine(pen, c.Anchor, (c.Anchor + v2 * (r / 3)));
        }
    }
}
