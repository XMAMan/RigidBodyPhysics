using GraphicPanels;
using PhysicEngine.ExportData;
using PhysicEngine.RigidBody;
using System;
using System.Drawing;
using System.Linq;

namespace Part2.Model.Simulator.SimulatorShape
{
    class SimulatorRectangle : ISimulatorShape
    {
        private RigidRectangle rigidRectangle;

        public IRigidBody PhysicModel { get; private set; }

        public SimulatorRectangle(RectangleExportData ctor)
        {
            this.PhysicModel = this.rigidRectangle = new RigidRectangle(ctor);
        }
        public void Draw(GraphicPanel2D panel)
        {
            var r = this.rigidRectangle;

            //float angleInDegree = (float)(r.Angle / (2 * Math.PI) * 360);
            //panel.DrawFillRectangle(Color.Black, r.Center.Xi, r.Center.Yi, r.Size.Xi, r.Size.Yi, angleInDegree);

            panel.DrawPolygon(Pens.Black, r.Vertex.ToList());
        }

        public IExportShape GetConstructorData()
        {
            return this.rigidRectangle.GetExportData();
        }
    }
}
