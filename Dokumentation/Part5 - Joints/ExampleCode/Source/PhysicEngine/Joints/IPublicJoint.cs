using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Joints
{
    public interface IPublicJoint
    {
        public enum SpringParameter { FrequenceyAndDampingRatio, StiffnessAndDamping, NoSoftness }
        public enum AngularMotor { Disabled, SpinAround, GoToReferenceAngle }
        public enum TranslationMotor { Disabled, IsMoving, GoToReferencePosition }

        IPublicRigidBody Body1 { get; }
        IPublicRigidBody Body2 { get; }
        Vec2D Anchor1 { get; }
        Vec2D Anchor2 { get; }
    }
    public interface IPublicDistanceJoint : IPublicJoint
    {
        float LengthPosition { get; set; }  //Sollwertlänge 0..1
        bool LimitIsEnabled { get; }
        float CurrentPosition { get; } //Istwert 0..1 -> 0=Length=MinLength; 1=Length=MaxLength
    }
    public interface IPublicRevoluteJoint : IPublicJoint 
    {
        AngularMotor Motor { get; set; }
        float MotorSpeed { get; set; }
        float MotorPosition { get; set; } //Sollwert 0..1
        float MaxMotorTorque { get; set; }

        float CurrentPosition { get; } //Istwert 0..1 -> 0=Angle=MinAngle; 1=Angle=MaxAngle
    }

    public interface IPublicPrismaticJoint : IPublicJoint
    {
        bool LimitIsEnabled { get; }        

        TranslationMotor Motor { get; set; }
        float MotorSpeed { get; set; }
        float MotorPosition { get; set; } //Sollwert 0..1 0=MinTranslation; 1=MaxTranslation
        float MaxMotorForce { get; set; }

        float CurrentPosition { get; } //Istwert 0..1 -> 0=Position=MinTranslation; 1=Position=MaxTranslation
    }

    public interface IPublicWeldJoint : IPublicJoint
    {
        float Stiffness { get; set; }
        float Damping { get; set; }
    }

    public interface IPublicWheelJoint : IPublicJoint
    {
        bool LimitIsEnabled { get; }

        TranslationMotor Motor { get; set; }
        float MotorSpeed { get; set; }
        float MotorPosition { get; set; } //Sollwert 0..1 0=MinTranslation; 1=MaxTranslation
        float MaxMotorForce { get; set; }

        float CurrentPosition { get; } //Istwert 0..1 -> 0=Position=MinTranslation; 1=Position=MaxTranslation
    }
}
