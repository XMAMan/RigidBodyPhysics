using PhysicEngine.ExportData.Joints;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Joints
{
    public interface IExportableJoint
    {
        IExportJoint GetExportData(List<IRigidBody> bodies);
    }
}
