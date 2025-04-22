using GraphicMinimal;

namespace PhysicEngine.CollisionDetection.BroadPhase
{
    public interface IBoundingCircle
    {
        public Vector2D Center { get; }
        public float Radius { get; }
    }
}
