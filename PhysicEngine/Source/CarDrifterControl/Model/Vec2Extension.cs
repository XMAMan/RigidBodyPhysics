namespace CarDrifterControl.Model
{
    internal static class Vec2Extension
    {
        internal static GraphicMinimal.Vector2D ToGrx(this RigidBodyPhysics.MathHelper.Vec2D v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }
    }
}
