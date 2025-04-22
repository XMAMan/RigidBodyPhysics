using PhysicEngine.MathHelper;

namespace PhysicEngine.ExportData.Joints
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
        public float MaxForceToBreak { get; set; }
        
    }
}
