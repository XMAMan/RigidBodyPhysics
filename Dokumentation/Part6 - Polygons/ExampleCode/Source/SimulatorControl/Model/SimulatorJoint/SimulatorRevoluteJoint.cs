using GraphicPanels;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;

namespace SimulatorControl.Model.SimulatorJoint
{
    internal class SimulatorRevoluteJoint : ISimulatorJoint
    {
        private IPublicRevoluteJoint revoluteJoint;
        public IPublicJoint PhysicModel { get; }
        public SimulatorRevoluteJoint(IPublicRevoluteJoint ctor)
        {
            this.PhysicModel = this.revoluteJoint = ctor;
        }
        public void Draw(GraphicPanel2D panel, PrintSettings printSettings)
        {
            var c = this.revoluteJoint;

            if (printSettings.ShowJoints)
            {
                float radius = 10;
                Vec2D dir1 = (c.Body1.Center - c.Anchor1).Normalize();
                panel.DrawLine(Pens.Blue, (c.Anchor1 + dir1 * radius).ToGrx(), c.Body1.Center.ToGrx());

                Vec2D dir2 = (c.Body2.Center - c.Anchor2).Normalize();
                panel.DrawLine(Pens.Blue, (c.Anchor2 + dir2 * radius).ToGrx(), c.Body2.Center.ToGrx());

                panel.DrawCircle(Pens.Blue, c.Anchor1.ToGrx(), radius);
                panel.DrawFillCircle(Color.Blue, c.Anchor2.ToGrx(), radius / 2);
            }
                

            if (printSettings.ShowJointPosition)
                panel.DrawString(c.Anchor1.ToGrx(), Color.Black, 30, (int)(c.CurrentPosition * 100)+" " + (int)(c.MotorPosition * 100));
        }
    }
}
