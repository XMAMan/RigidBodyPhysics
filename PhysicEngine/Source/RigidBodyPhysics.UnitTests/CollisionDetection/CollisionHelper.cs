using RigidBodyPhysics.CollisionDetection;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.UnitTests.CollisionDetection
{
    internal static class CollisionHelper
    {
        internal static RigidBodyCollision[] GetAllCollisions(List<IRigidBody> bodies)
        {
            CollisionManager collisionManager = new CollisionManager(bodies, new bool[1, 1] { { true } });
            return collisionManager.GetAllCollisions();
        }
    }
}
