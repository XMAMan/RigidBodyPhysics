using PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Joints
{
    internal interface IJoint : IExportableJoint, IPublicJoint, ISISolvedJoint
    {
        IRigidBody B1 { get; }
        IRigidBody B2 { get; }
        Vec2D Anchor1 { get; } //Angabe in Weltkoordinaten
        Vec2D Anchor2 { get; } //Angabe in Weltkoordinaten
        bool CollideConnected { get; } //Wenn das falst ist kollidieren die Objekte B1 und B2 nicht miteinander
        void UpdateAnchorPoints(); //Muss aufgerufen werden, wenn sich die Position der Bodys geändert hat
    }

    internal interface IPointToPointJoint : IJoint
    {
        Vec2D AccumulatedPointToPointImpulse { get; set; }
    }

    internal interface IPointToLineJoint : IJoint
    {
        float AccumulatedPointToLineImpulse { get; set; }
        Vec2D B1ToA2 { get; } //d=Anchor2 - B1.Center
        Vec2D R1Dir { get; } //(Anchor1 - B1.Center).Normalize();
    }

    internal interface IMinMaxTranslationJoint : IJoint
    {
        float AccumulatedMinMaxImpulse { get; set; }
        float MinTranslation { get; }
        float MaxTranslation { get; }
        float R1Length { get; } //=R1.Length
        Vec2D B1ToA2 { get; } //d=Anchor2 - B1.Center
        Vec2D R1Dir { get; } //(Anchor1 - B1.Center).Normalize();
    }

    internal interface ITranslationMotorJoint : IJoint
    {
        bool LimitIsEnabled { get; }
        float MinTranslation { get; }
        float MaxTranslation { get; }

        float AccumulatedTranslationMotorImpulse { get; set; }
        TranslationMotor Motor { get; }
        float MotorSpeed { get; }
        float MotorPosition { get; } //Sollwertposition in Prozent zur Länge bei Simulationsstart
        float MaxMotorForce { get; }

        float R1Length { get; }
        Vec2D B1ToA2 { get; } //d=Anchor2 - B1.Center
        Vec2D R1Dir { get; } //(Anchor1 - B1.Center).Normalize();
        float MotorPixelPosition { get; } //Sollwertposition in Pixeln
        SoftConstraintData Soft { get; }
    }

    internal interface IFixAngularJoint : IJoint
    {
        float AccumulatedAngularImpulse { get; set; }
        float AngularDifferenceOnStart { get; }
        SoftConstraintData Soft { get; }
    }

    internal interface IMinMaxAngularJoint : IJoint
    {
        float AccumulatedMinMaxAngularImpulse { get; set; }
        float AngularDifferenceOnStart { get; }
        float DiffToMinOnStart { get; }
        float MinMaxDifference { get; }
    }


    internal interface IAngularMotorJoint : IJoint
    {
        float AccumulatedAngularMotorImpulse { get; set; }
        bool LimitIsEnabled { get; }
        float AngularDifferenceOnStart { get; }
        float DiffToMinOnStart { get; }
        float MinMaxDifference { get; }
        AngularMotor Motor { get; }
        float MotorSpeed { get; }
        float MotorPosition { get; }
        float MaxMotorTorque { get; }
        SoftConstraintData Soft { get; }
    }

    internal interface IPointToPointAndFixAngularJoint : IPointToPointJoint, IFixAngularJoint { }
}
