namespace EditorControl.Model
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

        internal static GraphicMinimal.Vector2D ToGrx(this PhysicEngine.MathHelper.Vec2D v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }

        internal static PhysicEngine.MathHelper.Vec2D ToPhx(this GraphicMinimal.Vector2D v)
        {
            return new PhysicEngine.MathHelper.Vec2D(v.X, v.Y);
        }

        internal static IEnumerable<GraphicMinimal.Vector2D> ToGrx(this IEnumerable<PhysicEngine.MathHelper.Vec2D> v)
        {
            return v.Select(x => new GraphicMinimal.Vector2D(x.X, x.Y)).ToList();
        }

        internal static IEnumerable<PhysicEngine.MathHelper.Vec2D> ToPhx(this IEnumerable<GraphicMinimal.Vector2D> v)
        {
            return v.Select(x => new PhysicEngine.MathHelper.Vec2D(x.X, x.Y));
        }
    }
}
