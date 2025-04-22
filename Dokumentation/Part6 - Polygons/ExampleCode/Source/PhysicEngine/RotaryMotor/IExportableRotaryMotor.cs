using PhysicEngine.ExportData.RotaryMotor;
using PhysicEngine.RigidBody;

namespace PhysicEngine.RotaryMotor
{
    internal interface IExportableRotaryMotor
    {
        IExportRotaryMotor GetExportData(List<IRigidBody> bodies);
    }
}
