using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionDetection
{
    public class RigidBodyCollision : CollisionInfo
    {
        public IRigidBody B1 { get; private set; }
        public IRigidBody B2 { get; private set; }

        public RigidBodyCollision(CollisionInfo info, IRigidBody b1, IRigidBody b2)
            :base(info.Start, info.End, info.Normal, info.Depth)
        {
            this.B1 = b1;
            this.B2 = b2;
        }
    }
}
