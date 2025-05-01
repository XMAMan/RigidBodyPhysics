using PhysicGlobal;

namespace PhysicSceneEditorControl.Controls.Editor.Model
{
    internal static class Vec2Extension
    {
        internal static GraphicMinimal.Vector2D ToGrx(this Point v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }

        internal static Point ToPoint(this GraphicMinimal.Vector2D v)
        {
            return new Point(v.Xi, v.Yi);
        }

        internal static GraphicMinimal.Vector2D ToGrx(this Vec2D v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }

        internal static Vec2D ToPhx(this GraphicMinimal.Vector2D v)
        {
            return new Vec2D(v.X, v.Y);
        }

        internal static IEnumerable<GraphicMinimal.Vector2D> ToGrx(this IEnumerable<Vec2D> v)
        {
            return v.Select(x => new GraphicMinimal.Vector2D(x.X, x.Y)).ToList();
        }

        internal static IEnumerable<Vec2D> ToPhx(this IEnumerable<GraphicMinimal.Vector2D> v)
        {
            return v.Select(x => new Vec2D(x.X, x.Y));
        }
    }
}
