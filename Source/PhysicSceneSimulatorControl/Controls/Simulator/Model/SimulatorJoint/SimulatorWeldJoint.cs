using GraphicPanels;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorJoint
{
    internal class SimulatorWeldJoint : ISimulatorJoint
    {
        private IPublicWeldJoint weldJoint;
        public IPublicJoint PhysicModel { get; }
        public SimulatorWeldJoint(IPublicWeldJoint ctor)
        {
            this.PhysicModel = this.weldJoint = ctor;
        }
        public void Draw(GraphicPanel2D panel, PrintSettingsViewModel printSettings)
        {
            var c = this.weldJoint;

            if (printSettings.ShowJoints)
            {
                int cornerCount = 7;
                float radius = 20;

                List<Vec2D> points = new List<Vec2D>();
                for (int i = 0; i < cornerCount; i++)
                {
                    points.Add(c.Anchor1 + new Vec2D((float)Math.Cos(i / (float)cornerCount * 2 * Math.PI), (float)Math.Sin(i / (float)cornerCount * 2 * Math.PI)) * radius);

                    panel.DrawLine(Pens.Blue, c.Anchor1.ToGrx(), points.Last().ToGrx());
                }

                panel.DrawPolygon(Pens.Blue, points.Select(x => x.ToGrx()).ToList());
            }
        }
    }
}
