using PhysicGlobal;

namespace DynamicObjCreation
{
    internal static class Vec2Extension
    {
        internal static Vec2D ToPhx(this GraphicMinimal.Vector2D v)
        {
            return new Vec2D(v.X, v.Y);
        }

        internal static GraphicMinimal.Vector2D ToGrx(this Vec2D v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }
    }
}
