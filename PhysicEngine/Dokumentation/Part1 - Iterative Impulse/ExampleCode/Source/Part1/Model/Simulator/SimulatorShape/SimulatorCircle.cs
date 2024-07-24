using GraphicMinimal;
using GraphicPanels;
using PhysicEngine.ExportData;
using PhysicEngine.RigidBody;
using System;
using System.Drawing;

namespace Part1.Model.Simulator.SimulatorShape
{
    class SimulatorCircle : ISimulatorShape
    {
        private RigidCircle rigidCircle;

        public IRigidBody PhysicModel { get; private set; }

        public SimulatorCircle(CircleExportData ctor)
        {
            this.PhysicModel = this.rigidCircle = new RigidCircle(ctor);
        }

        public void Draw(GraphicPanel2D panel)
        {
            var c = this.rigidCircle;
            panel.DrawCircle(Pens.Black, c.Center, c.Radius);

            Vector2D r = Vector2D.DirectionFromPhi(c.Angle);
            panel.DrawLine(Pens.Black, c.Center, c.Center + r * c.Radius);
        }

        public IExportShape GetConstructorData()
        {
            return this.rigidCircle.GetExportData();
        }
    }
}
