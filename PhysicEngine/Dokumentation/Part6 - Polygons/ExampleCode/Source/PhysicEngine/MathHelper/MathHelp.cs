using PhysicEngine.RigidBody;

namespace PhysicEngine.MathHelper
{
    static class MathHelp
    {
        internal static float Clamp(float a, float low, float high)
        {
            return Math.Max(low, Math.Min(a, high));
        }

        internal static Vec2D GetWorldPointFromLocalDirection(IRigidBody body, Vec2D localDirection)
        {
            Matrix2x2 toWorld = body.RotateToWorld;
            return body.Center + toWorld * localDirection;
        }

        internal static Vec2D GetLocalDirectionFromWorldPoint(IRigidBody body, Vec2D worldPosition)
        {
            Matrix2x2 toLocal = body.RotateToWorld.Transpose();
            return toLocal * (worldPosition - body.Center);
        }

        internal static Vec2D GetWorldDirectionFromLocalDirection(IRigidBody body, Vec2D localDirection)
        {
            Matrix2x2 toWorld = body.RotateToWorld;
            return toWorld * localDirection;
        }
    }
}
