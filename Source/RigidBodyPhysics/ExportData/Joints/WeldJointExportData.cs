using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.ExportData.Joints
{
    public class WeldJointExportData : IExportJoint
    {
        public int BodyIndex1 { get; set; }
        public int BodyIndex2 { get; set; }
        public Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor1-Punkt
        public Vec2D R2 { get; set; }
        public bool CollideConnected { get; set; }

        public SoftExportData SoftData { get; set; }

        public bool BreakWhenMaxForceIsReached { get; set; }
        public float MaxForceToBreak { get; set; }

        public WeldJointExportData() { }

        public WeldJointExportData(WeldJointExportData copy)
        {
            this.BodyIndex1 = copy.BodyIndex1;
            this.BodyIndex2 = copy.BodyIndex2;
            this.R1 = new Vec2D(copy.R1);
            this.R2 = new Vec2D(copy.R2);
            this.CollideConnected = copy.CollideConnected;
            this.SoftData = new SoftExportData(copy.SoftData);
            this.BreakWhenMaxForceIsReached = copy.BreakWhenMaxForceIsReached;
            this.MaxForceToBreak = copy.MaxForceToBreak;
        }

        public IExportJoint GetCopy()
        {
            return new WeldJointExportData(this);
        }
    }
}
