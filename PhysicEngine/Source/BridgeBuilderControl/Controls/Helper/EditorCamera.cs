using GraphicMinimal;
using System;
using System.Drawing;
using WpfControls.Controls.CameraSetting;

namespace BridgeBuilderControl.Controls.Helper
{

    //Erweitert die Camera2D um ZoomIn/ZoomOut/MoveCameraWithMouse-Funktion und sorgt dafür, dass die Kamera nicht über den Levelrand bewegt werden kann
    internal class EditorCamera : Camera2D
    {
        private uint xCount;
        private uint yCount;
        private int screenWidth;
        private int screenHeight;

        private float userZoom = 1; //Geht von 1..3 (1 = Ganzes Level ist zu sehen; 3 = nah herran gezoomt)

        public EditorCamera(int screenWidth, int screenHeight, uint xCount, uint yCount)
            :base(screenWidth, screenHeight, new RectangleF(0, 0, xCount, yCount))
        {
            this.xCount = xCount;
            this.yCount = yCount;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;

            SetInitialLevelSize();
        }

        public new void UpdateScreenSize(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            base.UpdateScreenSize(screenWidth, screenHeight);

            if (this.userZoom == 1)
            {
                SetInitialLevelSize();
            }
        }

        private void SetInitialLevelSize()
        {
            //Sorge dafür, dass wenn camera.Zoom == 1 ist, dann soll das ganze Level genau ins Fenster passen
            this.UpdateSceneBoundingBox(new RectangleF(0, 0, this.xCount, this.yCount));
            this.SetInitialCameraPosition();
            SetCameraZoom();
        }

        public void ZoomIn()
        {
            this.userZoom = Math.Min(3, this.userZoom + 0.01f);
            SetCameraZoom();
        }

        public void ZoomOut()
        {
            this.userZoom = Math.Max(1, this.userZoom - 0.01f); //Zoom muss größer 1 bleiben
            SetCameraZoom();
        }

        public void MoveCameraWithMouse(Vector2D screenMousePosition)
        {
            float border = 50;

            float x = 0, y = 0;

            if (screenMousePosition.X < border) x = -1;
            if (screenMousePosition.X > this.screenWidth - border) x = +1;
            if (screenMousePosition.Y < border) y = -1;
            if (screenMousePosition.Y > this.screenHeight - border) y = +1;

            MoveCamera(new Vector2D(x, y));
        }

        public void SetZoom(bool zoomInIsPressed, bool zoomOutIsPressed)
        {
            if (zoomInIsPressed)
            {
                this.ZoomIn();
            }
            if (zoomOutIsPressed)
            {
                this.ZoomOut();
            }
        }

        //Sorge dafür, dass immer ScaleX genutzt wird 
        private void SetCameraZoom()
        {
            var box = this.GetSceneBoundingBox().Value;
            float scaleX = this.screenWidth / box.Width;
            float scaleY = this.screenHeight / box.Height;
            if (scaleY > scaleX)
            {
                this.Zoom = this.userZoom;
            }
            else
            {
                float scaleFactor = GetScaleFactor(new SizeF(screenWidth, screenHeight), box.Size);
                this.Zoom = this.userZoom / scaleFactor * scaleX;
            }
        }

        //Bewegt die Kamera in XY-Richtung aber nur so weit, dass sie im Sichtbereich des Levels bleibt
        private void MoveCamera(Vector2D translate)
        {
            float step = 0.01f;
            this.X += this.GetScreenBox().Width * translate.X * step;
            this.Y += this.GetScreenBox().Height * translate.Y * step;

            //Sorge dafür, dass man die Kamera nicht außerhalb des Levelrands verschiebt
            //Der DrawingHelper zeichnet das Level im Bereich von (0..xCount; 0..yCount)
            var max = new Vector2D(xCount - xCount / this.userZoom, yCount - yCount / this.Zoom);

            if (this.X > max.X)
                this.X = max.X;
            if (this.Y > max.Y)
                this.Y = max.Y;

            if (this.X < 0)
                this.X = 0;
            if (this.Y < 0)
                this.Y = 0;
        }
    }
}
