using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.ExportData.RigidBody
{
    public class PropertysExportData
    {
        public Vec2D Center { get; set; }
        public float AngleInDegree { get; set; }
        public Vec2D Velocity { get; set; } = new Vec2D(0, 0);
        public float AngularVelocity { get; set; } = 0;
        public MassData MassData { get; set; } = null;
        public float Friction { get; set; } = 1;
        public float Restituion { get; set; } = 1;
        public int CollisionCategory { get; set; } = 0;

        public PropertysExportData() { }

        public PropertysExportData(PropertysExportData copy)
        {
            this.Center = new Vec2D(copy.Center);
            this.AngleInDegree = copy.AngleInDegree;
            this.Velocity = new Vec2D(copy.Velocity);
            this.AngularVelocity = copy.AngularVelocity;
            this.MassData = new MassData(copy.MassData);
            this.Friction = copy.Friction;
            this.Restituion = copy.Restituion;
            this.CollisionCategory = copy.CollisionCategory;
        }
    }

    public interface IPropertysExportData
    {
        Vec2D Center { get; set; }
        float AngleInDegree { get; set; }
        Vec2D Velocity { get; set; }
        float AngularVelocity { get; set; }
        MassData MassData { get; set; }
        float Friction { get; set; }
        float Restituion { get; set; }
        int CollisionCategory { get; set; }
    }
}
