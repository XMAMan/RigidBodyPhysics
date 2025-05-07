using PhysicGlobal;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorAxialFriction
{
    internal interface ISimulatorAxialFriction
    {
        IPublicAxialFriction PhysicModel { get; }
        void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings);
    }
}
