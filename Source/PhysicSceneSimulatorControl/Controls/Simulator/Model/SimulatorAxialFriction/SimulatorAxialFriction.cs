using GraphicPanels;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorAxialFriction
{
    internal class SimulatorAxialFriction : ISimulatorAxialFriction
    {
        public IPublicAxialFriction PhysicModel { get; }
        public SimulatorAxialFriction(IPublicAxialFriction ctor)
        {
            this.PhysicModel = ctor;
        }
        public void Draw(GraphicPanel2D panel, PrintSettingsViewModel printSettings)
        {
            var c = this.PhysicModel;

            if (printSettings.ShowAxialFrictions)
            {
                DrawStick(panel, c.Anchor, c.ForceDirection, Pens.Blue);
            }
        }

        private static void DrawStick(GraphicPanel2D panel, Vec2D position, Vec2D direction, Pen pen)
        {
            float r = 25;
            var p1 = position - direction * r;
            var p2 = position + direction * r;
            panel.DrawLine(pen, p1.ToGrx(), p2.ToGrx());

            int count = 5;
            float l = 10;
            Vec2D normal = direction.Spin90();
            for (int i = 0; i <= count; i++)
            {
                float f = (float)i / count;
                var p = (1 - f) * p1 + f * p2;
                panel.DrawLine(pen, (p - normal * l).ToGrx(), (p + normal * l).ToGrx());
            }
        }
    }
}
