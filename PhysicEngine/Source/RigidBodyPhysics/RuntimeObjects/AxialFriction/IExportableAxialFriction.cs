using RigidBodyPhysics.ExportData.AxialFriction;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.AxialFriction
{
    internal interface IExportableAxialFriction
    {
        IExportAxialFriction GetExportData(List<IRigidBody> bodies);
    }
}
