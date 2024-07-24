using GraphicMinimal;
using PhysicEngine.RigidBody;

namespace PhysicEngine.MathHelper
{
    public static class MathHelp
    {
        public static float Clamp(float a, float low, float high)
        {
            return Math.Max(low, Math.Min(a, high));
        }     
        
        public static Vector2D GetWorldPointFromLocalDirection(IRigidBody body, Vector2D localDirection)
        {
            Matrix2x2 toWorld = Matrix2x2.Rotate(body.Angle);
            return body.Center + toWorld * localDirection;
        }

        public static Vector2D GetLocalDirectionFromWorldPoint(IRigidBody body, Vector2D worldPosition)
        {
            Matrix2x2 toLocal = Matrix2x2.Rotate(-body.Angle);
            return toLocal * (worldPosition - body.Center);
        }

        public static Vector2D GetWorldDirectionFromLocalDirection(IRigidBody body, Vector2D localDirection)
        {
            Matrix2x2 toWorld = Matrix2x2.Rotate(body.Angle);
            return toWorld * localDirection;
        }
    }
}
