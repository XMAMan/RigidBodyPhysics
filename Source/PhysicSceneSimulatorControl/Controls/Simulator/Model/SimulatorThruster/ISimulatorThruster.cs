using PhysicGlobal;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorThruster
{
    internal interface ISimulatorThruster
    {
        IPublicThruster PhysicModel { get; }
        void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings);
    }
}
