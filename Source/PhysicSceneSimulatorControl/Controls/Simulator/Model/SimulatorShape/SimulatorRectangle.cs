using GraphicPanels;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorShape
{
    internal class SimulatorRectangle : ISimulatorShape
    {
        private IPublicRigidRectangle rigidRectangle;

        public IPublicRigidBody PhysicModel { get; private set; }

        public SimulatorRectangle(IPublicRigidRectangle ctor)
        {
            this.PhysicModel = this.rigidRectangle = ctor;
        }
        public void Draw(GraphicPanel2D panel, PrintSettingsViewModel printSettings)
        {
            var r = this.rigidRectangle;



            panel.DrawPolygon(Pens.Black, r.Vertex.ToGrx().ToList());

            if (printSettings.VisualizePushPullForce)
            {
                float f = Math.Min(1, Math.Max(-1, r.GetPushPullForce() / Math.Max(0.0001f, printSettings.MaxPushPullForce))); //f=-1 Maximal Zug; 0=Keine Zug/Druck; 1=Maximal Druck

                if (f == 0) return;

                GraphicMinimal.Vector3D color;
                if (f < 0)
                {
                    var c1 = new GraphicMinimal.Vector3D(0, 0, 1);
                    var c2 = new GraphicMinimal.Vector3D(0, 1, 0);
                    f += 1;
                    color = (1 - f) * c1 + f * c2;

                }
                else
                {
                    var c1 = new GraphicMinimal.Vector3D(0, 1, 0);
                    var c2 = new GraphicMinimal.Vector3D(1, 0, 0);
                    color = (1 - f) * c1 + f * c2;
                }

                float angleInDegree = (float)(r.Angle / (2 * Math.PI) * 360);
                panel.DrawFillRectangle(color.ToColor(), (int)r.Center.X, (int)r.Center.Y, (int)r.Size.X, (int)r.Size.Y, angleInDegree);
            }
        }
    }
}
