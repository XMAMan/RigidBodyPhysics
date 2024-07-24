using GraphicMinimal;
using System.Drawing;

namespace LevelEditorControl.EditorFunctions
{
    internal static class Vector2DExtensions
    {
        internal static Vector2D ToGrx(this PointF p)
        {
            return new Vector2D(p.X, p.Y);
        }

        internal static PointF ToPointF(this GraphicMinimal.Vector2D v)
        {
            return new PointF(v.X, v.Y);
        }
    }
}
