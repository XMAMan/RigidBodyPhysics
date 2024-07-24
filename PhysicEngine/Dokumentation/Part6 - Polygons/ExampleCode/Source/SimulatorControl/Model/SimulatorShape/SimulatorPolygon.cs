using GraphicPanels;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace SimulatorControl.Model.SimulatorShape
{
    internal class SimulatorPolygon : ISimulatorShape
    {
        private IPublicRigidPolygon rigidPolygon;

        public IPublicRigidBody PhysicModel { get; private set; }

        public SimulatorPolygon(IPublicRigidPolygon ctor)
        {
            this.PhysicModel = this.rigidPolygon = ctor;
        }
        public void Draw(GraphicPanel2D panel, PrintSettings printSettings)
        {
            var r = this.rigidPolygon;

            panel.DrawPolygon(Pens.Black, r.Vertex.ToGrx().ToList());

            //Testausgabe der konvexen Teilpolygone
            if (printSettings.ShowSubPolys)
            {
                for (int i = 0; i < this.rigidPolygon.SubPolys.Count; i++)
                {
                    var subPoly = this.rigidPolygon.SubPolys[i];
                    panel.DrawPolygon(Pens.Green, subPoly.ToGrx().ToList());
                    panel.DrawString(PolygonHelper.GetCenterOfMassFromPolygon(subPoly).ToGrx(), Color.Red, 20, i + "");
                    for (int j = 0; j < subPoly.Length; j++)
                    {
                        panel.DrawString((subPoly[j] + new Vec2D(i * 20, 0)).ToGrx(), Color.Red, 20, j + "");
                    }
                }

                //Konvexpunkte sind die Punkte, wo der Winkel zum Kanten größer 180 Grad ist (Bei InsidePoly innen gemessen; Bei OutsidePoly außen)
                if (this.rigidPolygon.IsConvex != null)
                {
                    for (int i=0;i<this.rigidPolygon.IsConvex.Length;i++)
                    {
                        if (this.rigidPolygon.IsConvex[i])
                        {
                            panel.DrawFillCircle(Color.Green, this.rigidPolygon.Vertex[i].ToGrx(), 5);
                        }
                        
                    }
                }
            }
            
        }
    }
}
