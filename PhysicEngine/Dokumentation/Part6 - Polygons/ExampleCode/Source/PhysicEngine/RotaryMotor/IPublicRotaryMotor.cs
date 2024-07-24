using PhysicEngine.RigidBody;

namespace PhysicEngine.RotaryMotor
{
    public interface IPublicRotaryMotor
    {
        IPublicRigidBody Body { get; }
        float RotaryForce { get; set; }
        bool IsEnabled { get; set; }
    }
}
