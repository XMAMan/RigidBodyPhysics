using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.Thruster
{
    public interface IPublicThruster
    {
        IPublicRigidBody Body { get; }
        Vec2D Anchor { get; }
        Vec2D ForceDirection { get; }
        float ForceLength { get; set; }
        bool IsEnabled { get; set; }
        event Action<bool> IsEnabledChanged;
    }
}
