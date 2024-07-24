using RigidBodyPhysics.ExportData.Thruster;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.Thruster
{
    internal interface IExportableThruster
    {
        IExportThruster GetExportData(List<IRigidBody> bodies);
    }
}
