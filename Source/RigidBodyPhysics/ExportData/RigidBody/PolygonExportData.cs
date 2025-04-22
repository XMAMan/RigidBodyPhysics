using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.ExportData.RigidBody
{
    public class PolygonExportData : PropertysExportData, IExportRigidBody
    {
        public PolygonCollisionType PolygonType { get; set; }
        public Vec2D[] Points { get; set; }

        public PolygonExportData() { }

        public PolygonExportData(PolygonExportData copy)
            : base(copy) 
        {
            PolygonType = copy.PolygonType;
            Points = copy.Points.Select(x => new Vec2D(x)).ToArray();
        }

        public IExportRigidBody GetCopy()
        {
            return new PolygonExportData(this);
        }
    }
}
