using PhysicEngine.CollisionDetection.BroadPhase;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionDetection
{
    public static class CollisionHelper
    {
        public static RigidBodyCollision[] GetAllCollisions(List<IRigidBody> bodies)
        {
            List<RigidBodyCollision> collisions = new List<RigidBodyCollision>();

            for (int i = 0; i < bodies.Count; i++)
                for (int j = i+1; j < bodies.Count; j++)
                {
                    var b1 = bodies[i];
                    var b2 = bodies[j];
                    if ((b1.InverseMass != 0 || b2.InverseMass != 0) && BoundingCircleTest.Collide(b1, b2))
                    {
                        var contacts = b1.CollideWith(b2);
                        if (contacts != null)
                            collisions.AddRange(contacts.Select(x => new RigidBodyCollision(x, b1, b2, i, j)));
                    }
                }

            return collisions.ToArray();
        }
    }
}
