using PhysicEngine.ExportData.Joints;
using PhysicEngine.ExportData.RigidBody;

namespace PhysicEngine.ExportData
{
    public class PhysicSceneExportData
    {
        public IExportRigidBody[] Bodies { get; set; }
        public IExportJoint[] Joints { get; set; }
    }
}
