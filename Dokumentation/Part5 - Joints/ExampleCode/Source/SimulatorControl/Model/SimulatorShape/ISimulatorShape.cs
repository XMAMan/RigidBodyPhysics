using GraphicPanels;
using PhysicEngine.RigidBody;

namespace SimulatorControl.Model.SimulatorShape
{
    internal interface ISimulatorShape
    {
        IPublicRigidBody PhysicModel { get; }
        void Draw(GraphicPanel2D panel);
    }
}
