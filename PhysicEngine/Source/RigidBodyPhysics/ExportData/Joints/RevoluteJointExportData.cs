using RigidBodyPhysics.MathHelper;
using static RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint;

namespace RigidBodyPhysics.ExportData.Joints
{
    public class RevoluteJointExportData : IExportJoint
    {
        public int BodyIndex1 { get; set; }
        public int BodyIndex2 { get; set; }
        public Vec2D R1 { get; set; } //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor1-Punkt
        public Vec2D R2 { get; set; }
        public bool CollideConnected { get; set; }

        public bool LimitIsEnabled { get; set; }
        public float LowerAngle { get; set; }
        public float UpperAngle { get; set; }


        public AngularMotor Motor { get; set; }
        public float MotorSpeed { get; set; }   //Wenn Motor==SpinAround dreht sich der Motor mit MotorSpeed und maximaler Kraft von MaxMotorTorque
        public float MaxMotorTorque { get; set; }//Wenn Motor==SpinAround oder Motor==GoToReferenceAngle dreht der Motor mit einer maximalen Kraft von MaxMotorTorque 

        public SoftExportData SoftData { get; set; }

        public bool BreakWhenMaxForceIsReached { get; set; }
        public float MaxForceToBreak { get; set; }

        public RevoluteJointExportData() { }

        public RevoluteJointExportData(RevoluteJointExportData copy)
        {
            this.BodyIndex1 = copy.BodyIndex1;
            this.BodyIndex2 = copy.BodyIndex2;
            this.R1 = new Vec2D(copy.R1);
            this.R2 = new Vec2D(copy.R2);
            this.CollideConnected = copy.CollideConnected;
            this.LimitIsEnabled = copy.LimitIsEnabled;
            this.LowerAngle = copy.LowerAngle;
            this.UpperAngle = copy.UpperAngle;
            this.Motor = copy.Motor;
            this.MotorSpeed = copy.MotorSpeed;
            this.MaxMotorTorque = copy.MaxMotorTorque;
            this.SoftData = new SoftExportData(copy.SoftData);
            this.BreakWhenMaxForceIsReached = copy.BreakWhenMaxForceIsReached;
            this.MaxForceToBreak = copy.MaxForceToBreak;
        }

        public IExportJoint GetCopy()
        {
            return new RevoluteJointExportData(this);
        }
    }
}
