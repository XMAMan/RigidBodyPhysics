using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.MathHelper
{
    public static class PublicRigidBodyExtensions
    {
        public static Vec2D GetWorldPointFromLocalDirection(this IPublicRigidBody body, Vec2D localDirection)
        {
            return MathHelp.GetWorldPointFromLocalDirection((IRigidBody)body, localDirection);
        }
    }
}
