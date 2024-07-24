using PhysicEngine.MathHelper;

namespace PhysicEngine.ExportData.Joints
{
    public interface IExportJoint
    {
        int BodyIndex1 { get; set; }
        int BodyIndex2 { get; set; }
        Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor1-Punkt
        Vec2D R2 { get; set; }
        bool CollideConnected { get; set; }
    }
}
