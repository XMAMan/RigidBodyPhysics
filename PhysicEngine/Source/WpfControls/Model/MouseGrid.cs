using GraphicMinimal;
using GraphicPanels;
using System.Drawing;

namespace WpfControls.Model
{
    //Zeichnet ein Gitter und läßt die Maus daran ausrichten
    public class MouseGrid
    {
        private readonly float Bias = 0.2f; //So viel Prozent ist der Snap-Einzugsbereich groß

        public bool ShowGrid = true;
        public uint Size = 50; //So viele Pixel ist ein Grid-Kästchen breit und hoch

        //cameraLeftTop = Linke obere Ecke der Kamera im Cameraspace
        public void Draw(GraphicPanel2D panel, float cameraFactor, Vector2D cameraLeftTop)
        {
            float screenWidth = (panel.Width * cameraFactor);
            float screenHeight = (panel.Height * cameraFactor);

            int xCount = (int)(screenWidth / Size) + 2;
            int yCount = (int)(screenHeight / Size) + 2;

            int xStart = (int)(cameraLeftTop.X / Size) - 1;
            int yStart = (int)(cameraLeftTop.Y / Size) - 1;

            for (int i = 0; i <= xCount; i++)
            {
                panel.DrawLine(Pens.LightGray, new Vector2D((i + xStart) * Size, 0 + yStart * Size), new Vector2D((i + xStart) * Size, screenHeight + yStart * Size));
            }

            for (int i = 0; i <= yCount; i++)
            {
                panel.DrawLine(Pens.LightGray, new Vector2D(0 + xStart * Size, (i + yStart) * Size), new Vector2D(screenWidth + xStart * Size, (i + yStart) * Size));
            }
        }

        public Vector2D SnapMouse(Vector2D position)
        {
            if (this.ShowGrid == false) return position;

            float f1 = position.X / Size;
            float f2 = position.Y / Size;

            float x = position.X;
            float y = position.Y;

            int xi = (int)(f1);
            int yi = (int)(f2);
            float xf = f1 - xi;
            float yf = f2 - yi;
            if (xf < Bias) x = xi * Size;
            if (xf > 1 - Bias) x = (xi + 1) * Size;

            if (yf < Bias) y = yi * Size;
            if (yf > 1 - Bias) y = (yi + 1) * Size;

            return new Vector2D(x, y);
        }
    }
}
