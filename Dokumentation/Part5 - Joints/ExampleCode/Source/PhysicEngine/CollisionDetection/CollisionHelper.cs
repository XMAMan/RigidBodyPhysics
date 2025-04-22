using PhysicEngine.CollisionDetection.BroadPhase;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionDetection
{
    internal static class CollisionHelper
    {
        internal static RigidBodyCollision[] GetAllCollisions(List<IRigidBody> bodies)
        {
            List<RigidBodyCollision> collisions = new List<RigidBodyCollision>();

            for (int i = 0; i < bodies.Count; i++)
                for (int j = i+1; j < bodies.Count; j++)
                {
                    var b1 = bodies[i];
                    var b2 = bodies[j];
                    if (ShouldCollide(b1, b2) && BoundingCircleTest.Collide(b1, b2))
                    {
                        var contacts = b1.CollideWith(b2);
                        if (contacts != null)
                            collisions.AddRange(contacts.Select(x => new RigidBodyCollision(x, b1, b2, i, j)));
                    }
                }

            return collisions.ToArray();
        }

        private static bool ShouldCollide(IRigidBody b1, IRigidBody b2)
        {
            if (b1.InverseMass == 0 && b2.InverseMass == 0) return false;

            if (b1.CollideExcludeList.Contains(b2)) return false;

            return true;
        }
    }
}
