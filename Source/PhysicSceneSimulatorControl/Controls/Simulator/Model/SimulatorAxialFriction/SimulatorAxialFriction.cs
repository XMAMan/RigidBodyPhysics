using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using PhysicGlobal;
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
        public void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings)
        {
            var c = this.PhysicModel;

            if (printSettings.ShowAxialFrictions)
            {
                DrawStick(panel, c.Anchor, c.ForceDirection, Pens.Blue);
            }
        }

        private static void DrawStick(IDrawingPanel panel, Vec2D position, Vec2D direction, Pen pen)
        {
            float r = 25;
            var p1 = position + direction * r;
            var p2 = position - direction * r;
            panel.DrawLine(pen, p1, p2);

            int count = 5;
            float l = 10;
            Vec2D normal = direction.Spin90();
            for (int i = 0; i <= count - 2; i++)
            {
                float f = (float)i / count;
                var p = (1 - f) * p1 + f * p2;
                panel.DrawLine(pen, (p - normal * l), (p + normal * l));
            }

            var v1 = Vec2D.GetV2FromAngle360(direction, 45 + 90);
            var v2 = Vec2D.GetV2FromAngle360(direction, -45 - 90);
            panel.DrawLine(pen, p2, (p2 - v1 * (r / 1.5f)));
            panel.DrawLine(pen, p2, (p2 - v2 * (r / 1.5f)));
        }
    }
}
