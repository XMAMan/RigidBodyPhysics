using GameHelper;
using GraphicMinimal;
using GraphicPanels;
using System;
using System.Drawing;

namespace MoonlanderControl.Model
{
    //Horizintale Linie von p1 nach p2, wo das Raumschiff landen kann
    class LandingArea
    {
        //private static ColorInterpolator colorInterpolator = new ColorInterpolator(new Color[] { Color.Yellow, Color.Green, Color.HotPink, Color.Yellow });
        private static ColorInterpolator colorInterpolator = new ColorInterpolator(new Color[] { Color.DarkBlue, Color.Yellow });


        private Vector2D p1, p2;

        private float radiusVelocity = 0.005f; //So schnell ändert sich der Radius von en Punkten
        private float colorVelocity = 10; //Mit der Geschwindigkeit ändert sich die Farbe

        private float time = 0;

        public float Length { get; } //Breite der Plattform

        public float MinX { get => this.p1.X; }
        public float MaxX { get => this.p2.X; }

        public Vector2D Center { get; }

        public LandingArea(Vector2D p1, Vector2D p2)
        {
            if (p1.Y != p2.Y) throw new ArgumentException("Linie must be horizontal");
            if (p2.X < p1.X) throw new ArgumentException("p2.X must be greater then p1.X");

            this.p1 = p1;
            this.p2 = p2;

            this.Length = p2.X - p1.X;
            this.Center = (p1 + p2) * 0.5f;
        }

        public void MoveOnStep(float dt)
        {
            this.time += radiusVelocity * dt;
        }

        public void Draw(GraphicPanel2D panel)
        {            
            float timeF = this.time / colorVelocity;
            timeF -= (int)timeF;

            float distance = 18; //So viele Pixel sind die Punkte voneinander entfernt
            int pointCount = (int)(this.Length / distance);
            for (int i = 0; i < pointCount; i++)
            {
                float x = p1.X + i * distance;
                float size = ((float)Math.Sin(time + x / 10) + 1) / 2;

                float col = timeF + i / (float)pointCount;
                if (col > 1) col -= 1;

                //panel.DrawPixel(new Vector2D(x, p1.Y), colorInterpolator.GetColor(col), 7 * size);
                //panel.DrawCircle(new Pen(colorInterpolator.GetColor(col), 1), new Vector2D(x, p1.Y), 7 * size);
                //panel.DrawFillCircle(colorInterpolator.GetColor(col), new Vector2D(x, p1.Y + 7), 7 * size);

                panel.DrawLine(new Pen(colorInterpolator.GetColor(col), 4), new Vector2D(x - 6, p1.Y), new Vector2D(x + 6, p1.Y));
            }


            panel.DrawLine(new Pen(Color.Red, 4), p1, p1 - new Vector2D(0, 15));
            panel.DrawLine(new Pen(Color.Red, 4), p2, p2 - new Vector2D(0, 15));
        }
    }
}
