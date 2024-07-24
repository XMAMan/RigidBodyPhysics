using GraphicPanels;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.Joints;
using PhysicEngine.RigidBody;
using System.Collections.Generic;
using System.Drawing;

namespace Part4.Model.Simulator.SimulatorJoint
{
    internal class SimulatorDistanceJoint : ISimulatorJoint
    {
        private DistanceJoint distanceJoint;
        public IJoint PhysicModel { get; }
        public SimulatorDistanceJoint(DistanceJointExportData ctor, List<IRigidBody> bodies)
        {
            this.PhysicModel = this.distanceJoint = new DistanceJoint(ctor, bodies);
        }
        public void Draw(GraphicPanel2D panel)
        {
            var c = this.distanceJoint;
            panel.DrawLine(Pens.Blue, c.Anchor1, c.Anchor2);
        }

        public IExportJoint GetConstructorData(List<IRigidBody> bodies)
        {
            return this.distanceJoint.GetExportData(bodies);
        }
    }
}
