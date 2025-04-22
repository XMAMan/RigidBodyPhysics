using PhysicEngine.MathHelper;
using static PhysicEngine.Joints.IPublicJoint;

namespace PhysicEngine.ExportData.Joints
{
    public class PrismaticJointExportData : IExportJoint
    {
        public int BodyIndex1 { get; set; }
        public int BodyIndex2 { get; set; }
        public Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor1-Punkt
        public Vec2D R2 { get; set; }
        public bool CollideConnected { get; set; }

        public bool LimitIsEnabled { get; set; } = false; //Wenn true, muss der Abstand innerhalb der MinLength/MaxLenght-Schranken liegen
        public float MinTranslation { get; set; } = 0;         //Prozentzahl (0..1) MinLengthInPixel=MinLength * AnchorPointDistanceByStart
        public float MaxTranslation { get; set; } = 2;         //Prozentzahl (0..1) MaxLengthInPixel=MaxLength * AnchorPointDistanceByStart


        public TranslationMotor Motor { get; set; }
        public float MotorSpeed { get; set; }   //Mit der Geschwindigkeit bewegen sich die Körper aufeinander zu/weg
        public float MotorPosition { get; set; }//Sollabstand der Ankerpunkte. Geht von MinLength bis MaxLength
        public float MaxMotorForce { get; set; }//Maximale Kraft des Motors

        public SoftExportData SoftData { get; set; }
    }
}
