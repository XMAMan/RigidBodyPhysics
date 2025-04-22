using PhysicEngine.MathHelper;
using static PhysicEngine.Joints.IPublicJoint;

namespace PhysicEngine.ExportData.Joints
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
        public float MotorPosition { get; set; }//Wenn Motor==GoToReferenceAngle läßt der Motor auf die Hebelarme eine Kraft wirken so dass gilt: Vec2D.Angle360(r1, r2)==MotorPosition
        public float MaxMotorTorque { get; set; }//Wenn Motor==SpinAround oder Motor==GoToReferenceAngle dreht der Motor mit einer maximalen Kraft von MaxMotorTorque 

        public SoftExportData SoftData { get; set; }
    }
}
