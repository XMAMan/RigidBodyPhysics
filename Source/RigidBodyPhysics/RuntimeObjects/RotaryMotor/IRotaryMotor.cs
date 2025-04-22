using RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.RotaryMotor
{
    internal interface IRotaryMotor : IExportableRotaryMotor, IPublicRotaryMotor, ISISolvedRuntimeObject
    {
        IRigidBody B1 { get; }
        float AccumulatedBrakeImpulse { get; set; }
        bool IsEnabled { get; }
        event Action<bool> IsEnabledChanged;
        bool BrakeIsEnabled { get; }
        float TargetAngluarValueForBrake { get; }
    }
}
