using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.ExportData.AxialFriction
{
    public interface IExportAxialFriction
    {
        int BodyIndex { get; set; }
        Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor-Punkt
        Vec2D ForceDirection { get; set; }
        float Friction { get; set; }

        IExportAxialFriction GetCopy();
    }
}
