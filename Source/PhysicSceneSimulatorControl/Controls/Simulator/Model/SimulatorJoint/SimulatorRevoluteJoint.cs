using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using PhysicGlobal;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorJoint
{
    internal class SimulatorRevoluteJoint : ISimulatorJoint
    {
        private IPublicRevoluteJoint revoluteJoint;
        public IPublicJoint PhysicModel { get; }
        public SimulatorRevoluteJoint(IPublicRevoluteJoint ctor)
        {
            this.PhysicModel = this.revoluteJoint = ctor;
        }
        public void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings)
        {
            var c = this.revoluteJoint;

            if (printSettings.ShowJoints)
            {
                float radius = 10;
                Vec2D dir1 = (c.Body1.Center - c.Anchor1).Normalize();
                panel.DrawLine(Pens.Blue, (c.Anchor1 + dir1 * radius), c.Body1.Center);

                Vec2D dir2 = (c.Body2.Center - c.Anchor2).Normalize();
                panel.DrawLine(Pens.Blue, (c.Anchor2 + dir2 * radius), c.Body2.Center);

                panel.DrawCircle(Pens.Blue, c.Anchor1, radius);
                panel.DrawFillCircle(Color.Blue, c.Anchor2, radius / 2);
            }


            if (printSettings.ShowJointPosition)
                panel.DrawString(c.Anchor1, Color.Black, 30, (int)(c.CurrentPosition * 100) + " " + (int)(c.MotorPosition * 100));
        }
    }
}
