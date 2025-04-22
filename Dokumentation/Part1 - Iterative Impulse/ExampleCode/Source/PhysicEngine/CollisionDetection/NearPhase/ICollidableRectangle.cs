using GraphicMinimal;

namespace PhysicEngine.CollisionDetection.NearPhase
{
    public interface ICollidableRectangle : ICollidable
    {
        #region Wird von RectangleRectangleCollision1 verwendet
        //0--TopLeft;1--TopRight;2--BottomRight;3--BottomLeft
        Vector2D[] Vertex { get; }

        //0--Top;1--Right;2--Bottom;3--Left
        Vector2D[] FaceNormal { get; }
        #endregion

        #region Wird von RectangleRectangleCollision2 verwendet
        Vector2D Center { get; }
        float Angle { get; }
        Vector2D Size { get; }
        #endregion
    }
}
