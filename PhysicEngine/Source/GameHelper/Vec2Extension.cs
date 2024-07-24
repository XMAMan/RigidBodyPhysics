using RigidBodyPhysics.MathHelper;

namespace GameHelper
{
    internal static class Vec2Extension
    {
        internal static RigidBodyPhysics.MathHelper.Vec2D ToPhx(this GraphicMinimal.Vector2D v)
        {
            return new RigidBodyPhysics.MathHelper.Vec2D(v.X, v.Y);
        }

        internal static GraphicMinimal.Vector2D ToGrx(this RigidBodyPhysics.MathHelper.Vec2D v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }

        internal static Vec2D ToPhx(this PointF p)
        {
            return new Vec2D(p.X, p.Y);
        }
    }
}
