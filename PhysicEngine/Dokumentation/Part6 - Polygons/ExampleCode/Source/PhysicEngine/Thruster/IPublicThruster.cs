using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Thruster
{
    public interface IPublicThruster
    {
        IPublicRigidBody Body { get; }
        Vec2D Anchor { get; }
        Vec2D ForceDirection { get; }
        float ForceLength { get; set; }
        bool IsEnabled { get; set; }
    }
}
