using PhysicEngine.MathHelper;

namespace PhysicEngine.ExportData.Thruster
{
    public interface IExportThruster
    {
        int BodyIndex { get; set; }
        Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor-Punkt
        Vec2D ForceDirection { get; set; }
        float ForceLength { get; set; }
        bool IsEnabled { get; set; }
    }
}
