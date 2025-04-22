using GraphicMinimal;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model
{
    internal static class Vec2Extension
    {
        internal static GraphicMinimal.Vector2D ToGrx(this RigidBodyPhysics.MathHelper.Vec2D v)
        {
            return new GraphicMinimal.Vector2D(v.X, v.Y);
        }

        internal static RigidBodyPhysics.MathHelper.Vec2D ToPhx(this GraphicMinimal.Vector2D v)
        {
            return new RigidBodyPhysics.MathHelper.Vec2D(v.X, v.Y);
        }

        internal static IEnumerable<GraphicMinimal.Vector2D> ToGrx(this IEnumerable<RigidBodyPhysics.MathHelper.Vec2D> v)
        {
            return v.Select(x => new GraphicMinimal.Vector2D(x.X, x.Y)).ToList();
        }

        internal static IEnumerable<RigidBodyPhysics.MathHelper.Vec2D> ToPhx(this IEnumerable<GraphicMinimal.Vector2D> v)
        {
            return v.Select(x => new RigidBodyPhysics.MathHelper.Vec2D(x.X, x.Y));
        }

        internal static Color ToColor(this Vector3D color)
        {
            color.X = Clamp(color.X, 0, 1);
            color.Y = Clamp(color.Y, 0, 1);
            color.Z = Clamp(color.Z, 0, 1);
            return Color.FromArgb((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255));
        }

        private static float Clamp(float f, float min, float max)
        {
            if (f < min) f = min;
            if (f > max) f = max;
            return f;
        }
    }
}
