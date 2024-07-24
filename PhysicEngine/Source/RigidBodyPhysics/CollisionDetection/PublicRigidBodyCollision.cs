using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.CollisionDetection
{
    public class PublicRigidBodyCollision
    {
        public IPublicRigidBody Body1 { get; }
        public IPublicRigidBody Body2 { get; }
        public Vec2D Start { get; }  //Collisionpoint from RigidBody1
        public Vec2D End { get; }    //Collisionpoint from RigidBody2

        internal PublicRigidBodyCollision(RigidBodyCollision col)
        {
            this.Body1 = col.B1;
            this.Body2 = col.B2;
            this.Start = col.Start;
            this.End = col.End;
        }
    }
}
