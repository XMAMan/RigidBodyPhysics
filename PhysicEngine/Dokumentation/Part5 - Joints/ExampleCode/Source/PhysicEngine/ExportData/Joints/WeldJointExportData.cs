using PhysicEngine.MathHelper;

namespace PhysicEngine.ExportData.Joints
{
    public class WeldJointExportData : IExportJoint
    {
        public int BodyIndex1 { get; set; }
        public int BodyIndex2 { get; set; }
        public Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor1-Punkt
        public Vec2D R2 { get; set; }
        public bool CollideConnected { get; set; }

        public SoftExportData SoftData { get; set; }
    }
}
