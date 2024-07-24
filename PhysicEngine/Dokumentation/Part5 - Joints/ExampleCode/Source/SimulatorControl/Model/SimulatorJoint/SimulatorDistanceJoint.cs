using GraphicPanels;
using PhysicEngine.Joints;

namespace SimulatorControl.Model.SimulatorJoint
{
    internal class SimulatorDistanceJoint : ISimulatorJoint
    {
        private IPublicDistanceJoint distanceJoint;
        public IPublicJoint PhysicModel { get; }
        public SimulatorDistanceJoint(IPublicDistanceJoint ctor)
        {
            this.PhysicModel = this.distanceJoint = ctor;
        }
        public void Draw(GraphicPanel2D panel)
        {
            var c = this.distanceJoint;
            panel.DrawLine(Pens.Blue, c.Anchor1.ToGrx(), c.Anchor2.ToGrx());

            //Testausgabe
            panel.DrawString(c.Anchor1.ToGrx(), Color.Black, 30, (int)(c.CurrentPosition * 100) + "");
        }
    }
}
