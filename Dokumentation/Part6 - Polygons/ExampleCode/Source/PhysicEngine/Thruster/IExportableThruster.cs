using PhysicEngine.ExportData.Thruster;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Thruster
{
    internal interface IExportableThruster
    {
        IExportThruster GetExportData(List<IRigidBody> bodies);
    }
}
