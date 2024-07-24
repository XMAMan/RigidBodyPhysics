using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.ExportData.Joints
{
    public class DistanceJointExportData : IExportJoint
    {
        public int BodyIndex1 { get; set; }
        public int BodyIndex2 { get; set; }
        public Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor1-Punkt
        public Vec2D R2 { get; set; }
        public bool CollideConnected { get; set; }

        public bool LimitIsEnabled { get; set; } = false;
        public bool JointIsRope { get; set; } //=true -> DistanceJoint verhält sich wie ein Seil; false = DistanceJoint ist eine Eisenstange
        public float MinLength { get; set; } = 0;   //Minimallänge in Pixeln
        public float MaxLength { get; set; } = 100; //Maximallänge in Pixeln

        public SoftExportData SoftData { get; set; }

        public bool BreakWhenMaxForceIsReached { get; set; }
        public float MinForceToBreak { get; set; }
        public float MaxForceToBreak { get; set; }

        public DistanceJointExportData() { }

        public DistanceJointExportData(DistanceJointExportData copy)
        {
            this.BodyIndex1 = copy.BodyIndex1;
            this.BodyIndex2 = copy.BodyIndex2;
            this.R1 = new Vec2D(copy.R1);
            this.R2 = new Vec2D(copy.R2);
            this.CollideConnected = copy.CollideConnected;
            this.LimitIsEnabled = copy.LimitIsEnabled;
            this.JointIsRope = copy.JointIsRope;
            this.MinLength = copy.MinLength;
            this.MaxLength = copy.MaxLength;
            this.SoftData = new SoftExportData(copy.SoftData);
            this.BreakWhenMaxForceIsReached = copy.BreakWhenMaxForceIsReached;
            this.MinForceToBreak = copy.MinForceToBreak;
            this.MaxForceToBreak = copy.MaxForceToBreak;
        }

        public IExportJoint GetCopy()
        {
            return new DistanceJointExportData(this);
        }
    }
}
