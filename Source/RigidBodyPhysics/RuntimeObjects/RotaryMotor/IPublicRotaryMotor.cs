using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.RotaryMotor
{
    public interface IPublicRotaryMotor
    {

        IPublicRigidBody Body { get; }
        float RotaryForce { get; set; }
        float MaxSpeed { get; set; }
        bool IsEnabled { get; set; }
        bool BrakeIsEnabled { get; set; }
    }
}
