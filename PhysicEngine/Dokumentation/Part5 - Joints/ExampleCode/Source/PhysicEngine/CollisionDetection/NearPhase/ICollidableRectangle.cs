using PhysicEngine.MathHelper;

namespace PhysicEngine.CollisionDetection.NearPhase
{
    internal interface ICollidableRectangle : ICollidable
    {
        //0--TopLeft;1--TopRight;2--BottomRight;3--BottomLeft
        Vec2D[] Vertex { get; }

        //0--Top;1--Right;2--Bottom;3--Left
        Vec2D[] FaceNormal { get; }
    }
}
