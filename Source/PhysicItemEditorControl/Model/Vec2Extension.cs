using GraphicMinimal;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PhysicItemEditorControl.Model
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

        internal static RigidBodyPhysics.MathHelper.Vec2D ToPhx(this GraphicMinimal.Vector2D v)
        {
            return new RigidBodyPhysics.MathHelper.Vec2D(v.X, v.Y);
        }

        internal static PointF ToPointF(this Vector2D v)
        {
            return new PointF(v.X, v.Y);
        }

        internal static Vector2D ToGrx(this PointF p)
        {
            return new Vector2D(p.X, p.Y);
        }
    }
}
