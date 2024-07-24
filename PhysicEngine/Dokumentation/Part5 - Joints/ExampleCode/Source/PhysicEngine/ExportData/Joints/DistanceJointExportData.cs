using PhysicEngine.MathHelper;
using static PhysicEngine.Joints.IPublicJoint;

namespace PhysicEngine.ExportData.Joints
{
    public class DistanceJointExportData : IExportJoint
    {
        public int BodyIndex1 { get; set; }
        public int BodyIndex2 { get; set; }
        public Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor1-Punkt
        public Vec2D R2 { get; set; }
        public bool CollideConnected { get; set; }

        public float LengthPosition { get; set; } = 1; //Die Sollwertlänge ergibt sich aus dem Abstand der Anchor-Punkte zum DistanceJoint-Definitionszeitpunkt mal diesen LengthFactor
        public bool LimitIsEnabled { get; set; } = false;
        public float MinLength { get; set; } = 0;
        public float MaxLength { get; set; } = 2;

        public SoftExportData SoftData { get; set; }
    }
}
