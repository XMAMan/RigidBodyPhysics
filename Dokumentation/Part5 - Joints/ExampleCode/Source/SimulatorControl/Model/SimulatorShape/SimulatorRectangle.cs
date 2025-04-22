using GraphicPanels;
using PhysicEngine.RigidBody;

namespace SimulatorControl.Model.SimulatorShape
{
    internal class SimulatorRectangle : ISimulatorShape
    {
        private IPublicRigidRectangle rigidRectangle;

        public IPublicRigidBody PhysicModel { get; private set; }

        public SimulatorRectangle(IPublicRigidRectangle ctor)
        {
            this.PhysicModel = this.rigidRectangle = ctor;
        }
        public void Draw(GraphicPanel2D panel)
        {
            var r = this.rigidRectangle;

            //float angleInDegree = (float)(r.Angle / (2 * Math.PI) * 360);
            //panel.DrawFillRectangle(Color.Black, r.Center.Xi, r.Center.Yi, r.Size.Xi, r.Size.Yi, angleInDegree);

            panel.DrawPolygon(Pens.Black, r.Vertex.ToGrx().ToList());
        }
    }
}
