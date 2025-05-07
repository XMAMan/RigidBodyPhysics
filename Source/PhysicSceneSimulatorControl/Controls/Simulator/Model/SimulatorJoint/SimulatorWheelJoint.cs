using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using PhysicGlobal;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorJoint
{
    internal class SimulatorWheelJoint : ISimulatorJoint
    {
        private IPublicWheelJoint wheelJoint;
        public IPublicJoint PhysicModel { get; }
        public SimulatorWheelJoint(IPublicWheelJoint ctor)
        {
            this.PhysicModel = this.wheelJoint = ctor;
        }
        public void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings)
        {
            var c = this.wheelJoint;

            if (printSettings.ShowJoints)
            {
                Vec2D center = c.Anchor1 + (c.Anchor2 - c.Anchor1) / 2;
                Vec2D vec2D = c.Anchor1 - center;
                Vec2D centerToA1 = vec2D;
                Vec2D a1ToTangent = Vec2D.CrossWithZ(centerToA1, 1).Normalize() * 10;

                Vec2D a1 = center + centerToA1;
                Vec2D a2 = center - centerToA1;
                panel.DrawLine(Pens.Blue, a1, a2);
                panel.DrawLine(Pens.Blue, (a1 - a1ToTangent), (a1 + a1ToTangent));
                panel.DrawCircle(Pens.Blue, a2, 15);
            }

            if (printSettings.ShowJointPosition)
                panel.DrawString(c.Anchor1, Color.Black, 30, (int)(c.CurrentPosition * 100) + "");
        }
    }
}
