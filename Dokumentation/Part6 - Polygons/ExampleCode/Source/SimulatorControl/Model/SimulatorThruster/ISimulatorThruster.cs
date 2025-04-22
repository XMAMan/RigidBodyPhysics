using GraphicPanels;
using PhysicEngine.Thruster;

namespace SimulatorControl.Model.SimulatorThruster
{
    internal interface ISimulatorThruster
    {
        IPublicThruster PhysicModel { get; }
        void Draw(GraphicPanel2D panel, PrintSettings printSettings);
    }
}
