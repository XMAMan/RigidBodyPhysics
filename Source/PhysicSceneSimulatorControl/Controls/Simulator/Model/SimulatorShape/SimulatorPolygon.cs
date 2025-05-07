using PhysicGlobal;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorShape
{
    internal class SimulatorPolygon : ISimulatorShape
    {
        private IPublicRigidPolygon rigidPolygon;

        public IPublicRigidBody PhysicModel { get; private set; }

        public SimulatorPolygon(IPublicRigidPolygon ctor)
        {
            this.PhysicModel = this.rigidPolygon = ctor;
        }
        public void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings)
        {
            var r = this.rigidPolygon;

            panel.DrawPolygon(Pens.Black, r.Vertex);

            //Testausgabe der konvexen Teilpolygone
            if (printSettings.ShowSubPolys)
            {
                for (int i = 0; i < this.rigidPolygon.SubPolys.Count; i++)
                {
                    var subPoly = this.rigidPolygon.SubPolys[i];
                    panel.DrawPolygon(Pens.Green, subPoly);
                    panel.DrawString(PolygonHelper.GetCenterOfMassFromPolygon(subPoly), Color.Red, 20, i + "");
                    for (int j = 0; j < subPoly.Length; j++)
                    {
                        panel.DrawString((subPoly[j] + new Vec2D(i * 20, 0)), Color.Red, 20, j + "");
                    }
                }

                //Konvexpunkte sind die Punkte, wo der Winkel zum Kanten größer 180 Grad ist (Bei InsidePoly innen gemessen; Bei OutsidePoly außen)
                if (this.rigidPolygon.IsConvex != null)
                {
                    for (int i = 0; i < this.rigidPolygon.IsConvex.Length; i++)
                    {
                        if (this.rigidPolygon.IsConvex[i])
                        {
                            panel.DrawFillCircle(Color.Green, this.rigidPolygon.Vertex[i], 5);
                        }

                    }
                }
            }

        }
    }
}
