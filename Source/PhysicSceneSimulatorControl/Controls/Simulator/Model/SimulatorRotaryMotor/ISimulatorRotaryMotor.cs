using PhysicGlobal;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorRotaryMotor
{
    internal interface ISimulatorRotaryMotor
    {
        IPublicRotaryMotor PhysicModel { get; }
        void Draw(IDrawingPanel panel, PrintSettingsViewModel printSettings);
    }
}
