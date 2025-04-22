using GraphicPanels;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.RigidBody;

namespace Part4.Model.Simulator.SimulatorShape
{
    interface ISimulatorShape
    {
        IRigidBody PhysicModel { get; }
        void Draw(GraphicPanel2D panel);
        IExportRigidBody GetConstructorData();
    }
}
