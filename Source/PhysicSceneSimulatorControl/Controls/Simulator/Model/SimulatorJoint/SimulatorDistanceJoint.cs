using PhysicGlobal;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorJoint
{
    internal class SimulatorDistanceJoint : ISimulatorJoint
    {
        private IPublicDistanceJoint distanceJoint;
        public IPublicJoint PhysicModel { get; }
        public SimulatorDistanceJoint(IPublicDistanceJoint ctor)
        {
            this.PhysicModel = this.distanceJoint = ctor;
        }
        public void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings)
        {
            var c = this.distanceJoint;

            if (printSettings.ShowJoints)
                panel.DrawLine(Pens.Blue, c.Anchor1, c.Anchor2);

            if (printSettings.ShowJointPosition)
                panel.DrawString(c.Anchor1, Color.Black, 30, (int)(c.CurrentPosition) + "");
        }
    }
}
