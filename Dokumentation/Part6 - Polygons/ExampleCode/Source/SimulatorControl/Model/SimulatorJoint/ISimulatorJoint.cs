using GraphicPanels;
using PhysicEngine.Joints;

namespace SimulatorControl.Model.SimulatorJoint
{
    internal interface ISimulatorJoint
    {
        IPublicJoint PhysicModel { get; }
        void Draw(GraphicPanel2D panel, PrintSettings printSettings);
    }
}
