using PhysicGlobal;

namespace RigidBodyPhysics.CollisionDetection.BroadPhase
{
    internal interface IBoundingCircle
    {
        Vec2D Center { get; }
        float Radius { get; }
    }
}
