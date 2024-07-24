using RigidBodyPhysics.ExportData.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.RotaryMotor
{
    internal interface IExportableRotaryMotor
    {
        IExportRotaryMotor GetExportData(List<IRigidBody> bodies);
    }
}
