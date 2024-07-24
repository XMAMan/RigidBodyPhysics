using GraphicMinimal;

namespace PhysicEngine.CollisionDetection.NearPhase
{
    public interface ICollidableCircle : ICollidable
    {
        public Vector2D Center { get; }
        public float Radius { get; }
    }
}
