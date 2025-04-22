using GraphicPanels;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;

namespace SimulatorControl.Model.SimulatorJoint
{
    internal class SimulatorPrismaticJoint : ISimulatorJoint
    {
        private IPublicPrismaticJoint prismaticJoint;
        public IPublicJoint PhysicModel { get; }
        public SimulatorPrismaticJoint(IPublicPrismaticJoint ctor)
        {
            this.PhysicModel = this.prismaticJoint = ctor;
        }
        public void Draw(GraphicPanel2D panel, PrintSettings printSettings)
        {
            var c = this.prismaticJoint;

            if (printSettings.ShowJoints)
            {
                Vec2D tangent = Vec2D.CrossWithZ(c.Anchor2 - c.Anchor1, 1).Normalize() * 10;

                var pen = Pens.Blue;

                //Hülse
                panel.DrawLine(pen, (c.Anchor1 - tangent).ToGrx(), (c.Anchor1 + tangent).ToGrx());
                panel.DrawLine(pen, (c.Anchor1 - tangent).ToGrx(), (c.Anchor2 - tangent).ToGrx());
                panel.DrawLine(pen, (c.Anchor1 + tangent).ToGrx(), (c.Anchor2 + tangent).ToGrx());

                //Stift
                panel.DrawLine(new Pen(pen.Color, pen.Width + 3), c.Anchor1.ToGrx(), c.Anchor2.ToGrx());
            }
               

            if (printSettings.ShowJointPosition)
                panel.DrawString(c.Anchor1.ToGrx(), Color.Black, 30, (int)(c.CurrentPosition * 100)+"");
        }
    }
}
