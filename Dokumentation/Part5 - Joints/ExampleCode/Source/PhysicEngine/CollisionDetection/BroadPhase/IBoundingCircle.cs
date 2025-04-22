using PhysicEngine.MathHelper;

namespace PhysicEngine.CollisionDetection.BroadPhase
{
    internal interface IBoundingCircle
    {
        Vec2D Center { get; }
        float Radius { get; }
    }
}
