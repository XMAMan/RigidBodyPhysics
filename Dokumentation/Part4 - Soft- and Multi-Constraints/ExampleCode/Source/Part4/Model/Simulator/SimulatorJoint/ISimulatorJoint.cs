using GraphicPanels;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.Joints;
using PhysicEngine.RigidBody;
using System.Collections.Generic;

namespace Part4.Model.Simulator.SimulatorJoint
{
    internal interface ISimulatorJoint
    {
        IJoint PhysicModel { get; }
        void Draw(GraphicPanel2D panel);
        IExportJoint GetConstructorData(List<IRigidBody> bodies);
    }
}
