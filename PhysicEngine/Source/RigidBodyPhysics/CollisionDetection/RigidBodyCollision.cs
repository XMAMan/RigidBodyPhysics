using RigidBodyPhysics.CollisionDetection.NearPhase;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.CollisionDetection
{
    public interface IRigidBodyCollision
    {
        Vec2D Start { get; }
        Vec2D End { get; }
        Vec2D Normal { get; }
    }

    class RigidBodyCollision : CollisionInfo, IRigidBodyCollision
    {
        internal IRigidBody B1 { get; private set; }
        internal IRigidBody B2 { get; private set; }
        internal readonly string Key; //Eindeutiger Schlüssel zwischen Edge/Face von Körper 1 und Edge/Face von Körper 2


        internal RigidBodyCollision(CollisionInfo info, IRigidBody b1, IRigidBody b2, int body1Index, int body2Index)
            : base(info.Start, info.End, info.Normal, info.Depth, info.StartEdgeIndex, info.EndEdgeIndex)
        {
            this.B1 = b1;
            this.B2 = b2;
            this.Key = body1Index + "_" + body2Index + "_" + info.StartEdgeIndex + "_" + info.EndEdgeIndex;
        }

        internal RigidBodyCollision(RigidBodyCollision copy)
            : base(copy.Start, copy.Normal, copy.Depth, copy.StartEdgeIndex, copy.EndEdgeIndex)
        {
            this.B1 = copy.B1;
            this.B2 = copy.B2;
            this.Key = copy.Key;
        }
    }
}
