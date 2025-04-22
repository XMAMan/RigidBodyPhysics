using PhysicEngine.MathHelper;

namespace PhysicEngine.CollisionDetection.NearPhase
{
    internal interface ICollidableCircle : ICollidable
    {
        Vec2D Center { get; }
        float Radius { get; }
    }
}
