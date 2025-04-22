using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.ExportData.Thruster
{
    public class ThrusterExportData : IExportThruster
    {
        public int BodyIndex { get; set; }
        public Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor-Punkt
        public Vec2D ForceDirection { get; set; }
        public float ForceLength { get; set; }
        public bool IsEnabled { get; set; }

        public ThrusterExportData() { }

        public ThrusterExportData(ThrusterExportData copy)
        {
            this.BodyIndex = copy.BodyIndex;
            this.R1 = new Vec2D(copy.R1);
            this.ForceDirection = new Vec2D(copy.ForceDirection);
            this.ForceLength = copy.ForceLength;
            this.IsEnabled = copy.IsEnabled;
        }

        public IExportThruster GetCopy()
        {
            return new ThrusterExportData(this);
        }
    }
}
