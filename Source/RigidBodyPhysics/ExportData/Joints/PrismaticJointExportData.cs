using RigidBodyPhysics.MathHelper;
using static RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint;

namespace RigidBodyPhysics.ExportData.Joints
{
    public class PrismaticJointExportData : IExportJoint
    {
        public int BodyIndex1 { get; set; }
        public int BodyIndex2 { get; set; }
        public Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor1-Punkt
        public Vec2D R2 { get; set; }
        public bool CollideConnected { get; set; }

        public bool LimitIsEnabled { get; set; } = false; //Wenn true, muss der Abstand innerhalb der MinLength/MaxLenght-Schranken liegen
        public float MinTranslation { get; set; } = -1;        //Faktor für R1-Length (float.Min..float.Max) MinLengthInPixel=MinLength * R1.Length
        public float MaxTranslation { get; set; } = 1;         //Faktor für R1-Length (float.Min..float.Max) MaxLengthInPixel=MaxLength * R1.Length


        public TranslationMotor Motor { get; set; }
        public float MotorSpeed { get; set; }   //Mit der Geschwindigkeit bewegen sich die Körper aufeinander zu/weg
        public float MaxMotorForce { get; set; }//Maximale Kraft des Motors

        public SoftExportData SoftData { get; set; }

        public bool BreakWhenMaxForceIsReached { get; set; }
        public float MaxForceToBreak { get; set; }

        public PrismaticJointExportData() { }

        public PrismaticJointExportData(PrismaticJointExportData copy)
        {
            this.BodyIndex1 = copy.BodyIndex1;
            this.BodyIndex2 = copy.BodyIndex2;
            this.R1 = new Vec2D(copy.R1);
            this.R2 = new Vec2D(copy.R2);
            this.CollideConnected = copy.CollideConnected;
            this.LimitIsEnabled = copy.LimitIsEnabled;
            this.MinTranslation = copy.MinTranslation;
            this.MaxTranslation = copy.MaxTranslation;
            this.Motor = copy.Motor;
            this.MotorSpeed = copy.MotorSpeed;
            this.MaxMotorForce = copy.MaxMotorForce;
            this.SoftData = new SoftExportData(copy.SoftData);
            this.BreakWhenMaxForceIsReached = copy.BreakWhenMaxForceIsReached;
            this.MaxForceToBreak = copy.MaxForceToBreak;
        }

        public IExportJoint GetCopy()
        {
            return new PrismaticJointExportData(this);
        }
    }
}
