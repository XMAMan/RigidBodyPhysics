using PhysicGlobal;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorJoint
{
    internal interface ISimulatorJoint
    {
        IPublicJoint PhysicModel { get; }
        void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings);
    }
}
