using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.Joints
{
    internal interface IExportableJoint
    {
        IExportJoint GetExportData(List<IRigidBody> bodies);
        void LoadExportData(IExportJoint joint); //Lädt die SetPosition und das IsBroken-Flag
    }
}
