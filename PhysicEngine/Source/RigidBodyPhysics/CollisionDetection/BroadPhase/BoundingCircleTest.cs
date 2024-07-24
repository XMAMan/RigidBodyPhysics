namespace RigidBodyPhysics.CollisionDetection.BroadPhase
{
    static class BoundingCircleTest
    {
        internal static bool Collide(IBoundingCircle c1, IBoundingCircle c2)
        {
            float d = (c1.Center - c2.Center).Length();
            return d < (c1.Radius + c2.Radius);
        }
    }
}
