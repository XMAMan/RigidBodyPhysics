using PhysicGlobal;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorJoint
{
    internal class SimulatorPrismaticJoint : ISimulatorJoint
    {
        private IPublicPrismaticJoint prismaticJoint;
        public IPublicJoint PhysicModel { get; }
        public SimulatorPrismaticJoint(IPublicPrismaticJoint ctor)
        {
            this.PhysicModel = this.prismaticJoint = ctor;
        }
        public void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings)
        {
            var c = this.prismaticJoint;

            if (printSettings.ShowJoints)
            {
                Vec2D tangent = Vec2D.CrossWithZ(c.Anchor2 - c.Anchor1, 1).Normalize() * 10;

                var pen = Pens.Blue;

                //Hülse
                panel.DrawLine(pen, (c.Anchor1 - tangent), (c.Anchor1 + tangent));
                panel.DrawLine(pen, (c.Anchor1 - tangent), (c.Anchor2 - tangent));
                panel.DrawLine(pen, (c.Anchor1 + tangent), (c.Anchor2 + tangent));

                //Stift
                panel.DrawLine(new Pen(pen.Color, pen.Width + 3), c.Anchor1, c.Anchor2);
            }


            if (printSettings.ShowJointPosition)
                panel.DrawString(c.Anchor1, Color.Black, 30, (int)(c.CurrentPosition * 100) + "");
        }
    }
}
