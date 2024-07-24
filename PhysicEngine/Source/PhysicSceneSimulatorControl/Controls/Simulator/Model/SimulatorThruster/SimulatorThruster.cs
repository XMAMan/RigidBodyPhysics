using GraphicPanels;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.MathHelper;
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
        public void Draw(GraphicPanel2D panel, PrintSettingsViewModel printSettings)
        {
            var c = this.PhysicModel;

            if (printSettings.ShowThrusters)
            {
                DrawArrow(panel, c.IsEnabled ? Pens.Red : Pens.Blue);
            }
        }

        private void DrawArrow(GraphicPanel2D panel, Pen pen)
        {
            var c = this.PhysicModel;

            float r = 50;
            Vec2D forceDirection = c.ForceDirection;
            var v1 = GraphicMinimal.Vector2D.GetV2FromAngle360(forceDirection.ToGrx(), 45 + 90);
            var v2 = GraphicMinimal.Vector2D.GetV2FromAngle360(forceDirection.ToGrx(), -45 - 90);

            panel.DrawLine(pen, (c.Anchor - forceDirection * r).ToGrx(), c.Anchor.ToGrx());
            panel.DrawLine(pen, c.Anchor.ToGrx(), c.Anchor.ToGrx() + v1 * (r / 3));
            panel.DrawLine(pen, c.Anchor.ToGrx(), c.Anchor.ToGrx() + v2 * (r / 3));
        }
    }
}
