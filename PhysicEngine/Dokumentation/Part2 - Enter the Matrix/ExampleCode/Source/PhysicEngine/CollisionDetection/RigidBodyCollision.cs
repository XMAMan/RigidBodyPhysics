using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionDetection
{
    public class RigidBodyCollision : CollisionInfo
    {
        public IRigidBody B1 { get; private set; }
        public IRigidBody B2 { get; private set; }
        public int Body1Index { get; private set; }
        public int Body2Index { get; private set; }

        public RigidBodyCollision(CollisionInfo info, IRigidBody b1, IRigidBody b2, int body1Index, int body2Index)
            :base(info.Start, info.End, info.Normal, info.Depth)
        {
            this.B1 = b1;
            this.B2 = b2;
            this.Body1Index = body1Index;
            this.Body2Index = body2Index;
        }

        public RigidBodyCollision(RigidBodyCollision copy)
            :base(copy.Start, copy.Normal, copy.Depth)
        {
            this.B1 = copy.B1;
            this.B2 = copy.B2;
            this.Body1Index=copy.Body1Index;
            this.Body2Index=copy.Body2Index;
        }
    }
}
