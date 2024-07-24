using GraphicPanels;
using PhysicEngine.ExportData;
using PhysicEngine.RigidBody;

namespace Part3.Model.Simulator.SimulatorShape
{
    interface ISimulatorShape
    {
        IRigidBody PhysicModel { get; }
        void Draw(GraphicPanel2D panel);
        IExportShape GetConstructorData();
    }
}
