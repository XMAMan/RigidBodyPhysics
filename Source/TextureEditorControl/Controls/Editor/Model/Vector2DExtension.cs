using GraphicMinimal;
using System.Drawing;

namespace TextureEditorControl.Controls.Editor.Model
{
    internal static class Vector2DExtension
    {
        internal static PointF ToPointF(this Vector2D v)
        {
            return new PointF(v.X, v.Y);
        }

        internal static Vector2D ToGrx(this PointF p)
        {
            return new Vector2D(p.X, p.Y);
        }

        internal static Vector2D Spin90(this Vector2D dir)
        {
            return new Vector2D(-dir.Y, dir.X);
        }
    }
}
