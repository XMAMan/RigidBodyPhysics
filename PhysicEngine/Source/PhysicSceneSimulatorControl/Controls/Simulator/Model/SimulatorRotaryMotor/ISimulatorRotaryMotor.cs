using GraphicPanels;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorRotaryMotor
{
    internal interface ISimulatorRotaryMotor
    {
        IPublicRotaryMotor PhysicModel { get; }
        void Draw(GraphicPanel2D panel, PrintSettingsViewModel printSettings);
    }
}
