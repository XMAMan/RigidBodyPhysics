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
        float LengthPosition { get; set; }  //Sollwertlänge in Pixeln
        float MinLength { get; } //Minimallänge in Pixeln
        float MaxLength { get; } //Maximallänge in Pixeln
        bool LimitIsEnabled { get; }
        float CurrentPosition { get; } //Istwert in Pixeln MinLength..MaxLength (Abstand der beiden Ankerpunkte)
        float AccumulatedImpulse { get; }
        float AccumulatedImpulseForMinMax { get; }
    }
    public interface IPublicRevoluteJoint : IPublicJoint 
    {
        bool LimitIsEnabled { get; }
        AngularMotor Motor { get; set; }
        float MotorSpeed { get; set; }
        float MotorPosition { get; set; } //Sollwert 0..1
        float MaxMotorTorque { get; set; }

        float CurrentPosition { get; } //Istwert 0..1 -> 0=Angle=MinAngle; 1=Angle=MaxAngle

        float AccumulatedMinMaxAngularImpulse { get; }
        float AccumulatedAngularMotorImpulse { get; }
        Vec2D AccumulatedPointToPointImpulse { get; }
    }

    public interface IPublicPrismaticJoint : IPublicJoint
    {
        bool LimitIsEnabled { get; }        

        TranslationMotor Motor { get; set; }
        float MotorSpeed { get; set; }
        float MotorPosition { get; set; } //Sollwert 0..1 0=MinTranslation; 1=MaxTranslation
        float MaxMotorForce { get; set; }

        float CurrentPosition { get; } //Istwert 0..1 -> 0=Position=MinTranslation; 1=Position=MaxTranslation

        float AccumulatedPointToLineImpulse { get; }
        float AccumulatedAngularImpulse { get; }
        float AccumulatedMinMaxImpulse { get; }
        float AccumulatedTranslationMotorImpulse { get; }
    }

    public interface IPublicWeldJoint : IPublicJoint
    {
        float Stiffness { get; set; }
        float Damping { get; set; }

        Vec2D AccumulatedPointToPointImpulse { get; }
        float AccumulatedAngularImpulse { get; }
    }

    public interface IPublicWheelJoint : IPublicJoint
    {
        bool LimitIsEnabled { get; }

        TranslationMotor Motor { get; set; }
        float MotorSpeed { get; set; }
        float MotorPosition { get; set; } //Sollwert 0..1 0=MinTranslation; 1=MaxTranslation
        float MaxMotorForce { get; set; }

        float CurrentPosition { get; } //Istwert 0..1 -> 0=Position=MinTranslation; 1=Position=MaxTranslation

        float AccumulatedPointToLineImpulse { get;}
        float AccumulatedMinMaxImpulse { get;}
        float AccumulatedTranslationMotorImpulse { get;}
    }
}
