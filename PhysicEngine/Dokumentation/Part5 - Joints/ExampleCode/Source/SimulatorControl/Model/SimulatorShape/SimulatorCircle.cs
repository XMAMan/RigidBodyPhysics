using GraphicPanels;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace SimulatorControl.Model.SimulatorShape
{
    internal class SimulatorCircle : ISimulatorShape
    {
        private IPublicRigidCircle rigidCircle;

        public IPublicRigidBody PhysicModel { get; private set; }

        public SimulatorCircle(IPublicRigidCircle ctor)
        {
            this.PhysicModel = this.rigidCircle = ctor;
        }

        public void Draw(GraphicPanel2D panel)
        {
            var c = this.rigidCircle;
            panel.DrawCircle(Pens.Black, c.Center.ToGrx(), c.Radius);

            Vec2D r = Vec2D.DirectionFromPhi(c.Angle);
            panel.DrawLine(Pens.Black, c.Center.ToGrx(), (c.Center + r * c.Radius).ToGrx());
        }
    }
}
