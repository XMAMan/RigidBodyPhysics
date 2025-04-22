using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.ExportData.RigidBody
{
    public class PolygonExportData : PropertysExportData, IExportRigidBody
    {
        public PolygonCollisionType PolygonType { get; set; }
        public Vec2D[] Points { get; set; }        
    }
}
