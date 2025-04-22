using GraphicPanels;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorShape
{
    internal interface ISimulatorShape
    {
        IPublicRigidBody PhysicModel { get; }
        void Draw(GraphicPanel2D panel, PrintSettingsViewModel printSettings);
    }
}
