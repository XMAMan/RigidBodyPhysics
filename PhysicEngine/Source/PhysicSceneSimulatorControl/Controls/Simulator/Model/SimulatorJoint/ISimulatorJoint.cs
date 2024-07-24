using GraphicPanels;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorJoint
{
    internal interface ISimulatorJoint
    {
        IPublicJoint PhysicModel { get; }
        void Draw(GraphicPanel2D panel, PrintSettingsViewModel printSettings);
    }
}
