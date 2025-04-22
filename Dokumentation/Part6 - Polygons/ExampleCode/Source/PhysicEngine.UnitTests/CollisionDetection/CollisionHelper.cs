using PhysicEngine.CollisionDetection;
using PhysicEngine.RigidBody;

namespace PhysicEngine.UnitTests.CollisionDetection
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
