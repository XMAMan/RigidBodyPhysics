using PhysicEngine.ExportData.Joints;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.ExportData.RotaryMotor;
using PhysicEngine.ExportData.Thruster;

namespace PhysicEngine.ExportData
{
    public class PhysicSceneExportData
    {
        public IExportRigidBody[] Bodies { get; set; }
        public IExportJoint[] Joints { get; set; }
        public IExportThruster[] Thrusters { get; set; }
        public IExportRotaryMotor[] Motors { get; set; }
        public bool[,] CollisionMatrix { get; set; }
    }
}
