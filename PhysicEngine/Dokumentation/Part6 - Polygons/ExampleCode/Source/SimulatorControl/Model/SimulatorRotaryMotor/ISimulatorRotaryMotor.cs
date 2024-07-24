using GraphicPanels;
using PhysicEngine.RotaryMotor;

namespace SimulatorControl.Model.SimulatorRotaryMotor
{
    internal interface ISimulatorRotaryMotor
    {
        IPublicRotaryMotor PhysicModel { get; }
        void Draw(GraphicPanel2D panel, PrintSettings printSettings);
    }
}
