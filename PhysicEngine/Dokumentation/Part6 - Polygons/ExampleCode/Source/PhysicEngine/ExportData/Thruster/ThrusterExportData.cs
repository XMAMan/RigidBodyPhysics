using PhysicEngine.MathHelper;

namespace PhysicEngine.ExportData.Thruster
{
    public class ThrusterExportData : IExportThruster
    {
        public int BodyIndex { get; set; }
        public Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor-Punkt
        public Vec2D ForceDirection { get; set; }
        public float ForceLength { get; set; }
        public bool IsEnabled { get; set; }
    }
}
