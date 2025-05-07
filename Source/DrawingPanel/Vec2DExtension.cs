using PhysicGlobal;

namespace DrawingPanel
{
    internal static class Vec2DExtension
    {
        public static GraphicMinimal.Vector2D ToGrx(this Vec2D v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }

        public static IEnumerable<GraphicMinimal.Vector2D> ToGrx(this IEnumerable<Vec2D> v)
        {
            return v.Select(x => new GraphicMinimal.Vector2D(x.X, x.Y)).ToList();
        }
    }
}
