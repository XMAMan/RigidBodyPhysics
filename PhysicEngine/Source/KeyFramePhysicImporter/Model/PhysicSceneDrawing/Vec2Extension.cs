namespace KeyFramePhysicImporter.Model.PhysicSceneDrawing
{
    internal static class Vec2Extension
    {
        internal static GraphicMinimal.Vector2D ToGrx(this RigidBodyPhysics.MathHelper.Vec2D v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }

        internal static IEnumerable<GraphicMinimal.Vector2D> ToGrx(this IEnumerable<RigidBodyPhysics.MathHelper.Vec2D> v)
        {
            return v.Select(x => new GraphicMinimal.Vector2D(x.X, x.Y)).ToList();
        }

        internal static RigidBodyPhysics.MathHelper.Vec2D ToPhx(this PointF p)
        {
            return new RigidBodyPhysics.MathHelper.Vec2D(p.X, p.Y);
        }

        internal static GraphicMinimal.Vector2D ToGrx(this PointF p)
        {
            return new GraphicMinimal.Vector2D(p.X, p.Y);
        }

        internal static PointF ToPointF(this RigidBodyPhysics.MathHelper.Vec2D v)
        {
            return new PointF(v.X, v.Y);
        }

        internal static RectangleF ToRectangleF(this RigidBodyPhysics.MathHelper.BoundingBox box)
        {
            return new RectangleF(box.Min.X, box.Min.Y, box.Width, box.Height);
        }
    }
}
