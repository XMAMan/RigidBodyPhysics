using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.AxialFriction
{
    public interface IPublicAxialFriction
    {
        IPublicRigidBody Body { get; }
        Vec2D Anchor { get; }
        Vec2D ForceDirection { get; }
        float Friction { get; set; }
        float AccumulatedFrictionImpulse { get; }
    }
}
