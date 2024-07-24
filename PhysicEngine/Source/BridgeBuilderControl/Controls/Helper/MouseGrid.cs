using GraphicMinimal;
using GraphicPanels;
using System.Drawing;

namespace BridgeBuilderControl.Controls.Helper
{
    //Snapt die Mouse an das Gitter
    internal class MouseGrid
    {
        private static float Bias = 0.5f; //So viel Prozent ist der Snap-Einzugsbereich groß

        private GraphicPanel2D panel;

        public uint XCount { get; set; }


        //So viele Pixel ist ein Kästchen breit/hoch
        public float Size
        {
            get => panel.Width / (float)this.XCount;
        }

        public MouseGrid(GraphicPanel2D panel, uint xCount)
        {
            this.panel = panel;
            this.XCount = xCount;
        }

        public Point SnapToInt(Vector2D position)
        {
            return SnapToInteger(this.Size, position);
        }

        public static Point SnapToInteger(Vector2D position)
        {
            return SnapToInteger(1, position);
        }

        private static Point SnapToInteger(float size, Vector2D position)
        {
            var snapPoint = SnapMouse(size, position);

            return new Point((int)(snapPoint.X / size + 0.5f), (int)(snapPoint.Y / size + 0.5f));
        }

        //size = so groß ist ein Kästchen
        private static Vector2D SnapMouse(float size, Vector2D position)
        {
            float f1 = position.X / size;
            float f2 = position.Y / size;

            float x = position.X;
            float y = position.Y;

            int xi = (int)(f1);
            int yi = (int)(f2);
            float xf = f1 - xi;
            float yf = f2 - yi;
            if (xf < Bias) x = xi * size;
            if (xf > 1 - Bias) x = (xi + 1) * size;

            if (yf < Bias) y = yi * size;
            if (yf > 1 - Bias) y = (yi + 1) * size;

            return new Vector2D(x, y);
        }
    }
}
