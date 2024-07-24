using PhysicEngine.ExportData.Joints;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Joints
{
    internal interface IExportableJoint
    {
        IExportJoint GetExportData(List<IRigidBody> bodies);
    }
}
