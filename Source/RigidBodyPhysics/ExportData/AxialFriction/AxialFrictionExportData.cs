using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.ExportData.AxialFriction
{
    public class AxialFrictionExportData : IExportAxialFriction
    {
        public int BodyIndex { get; set; }
        public Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor-Punkt
        public Vec2D ForceDirection { get; set; }
        public float Friction { get; set; }

        public AxialFrictionExportData() { }

        public AxialFrictionExportData(AxialFrictionExportData copy)
        {
            this.BodyIndex = copy.BodyIndex;
            this.R1 = new Vec2D(copy.R1);
            this.ForceDirection = new Vec2D(copy.ForceDirection);
            this.Friction = copy.Friction;
        }

        public IExportAxialFriction GetCopy()
        {
            return new AxialFrictionExportData(this);
        }
    }
}
