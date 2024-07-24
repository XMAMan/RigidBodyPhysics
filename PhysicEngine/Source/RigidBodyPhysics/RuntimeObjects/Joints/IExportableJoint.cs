using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.Joints
{
    internal interface IExportableJoint
    {
        IExportJoint GetExportData(List<IRigidBody> bodies);
    }
}
