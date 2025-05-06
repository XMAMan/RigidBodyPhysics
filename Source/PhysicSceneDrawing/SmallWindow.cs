using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;
using PhysicGlobal;

namespace PhysicSceneDrawing
{
    //Zeichnet das kleine Vorschaubild rechts unten in der Ecke.
    //Es benutzt dazu den Zustand der LevelItems und der Kamera und verändert auch die Kameraposition/Zoom
    public class SmallWindow : ISizeChangeable
    {
        private readonly float size = 0.1f; //So viel Prozent vom Bildschirm ist das Fenster hier groß
        private readonly Color backColor = Color.FromArgb(153, 217, 234);
        private readonly Color frontColor = Color.FromArgb(63, 72, 204);

        private bool mouseIsDown;
        private Vec2D delta; //Vektor von MouseDownPosition zur linken oberen Ecke von der CameraBox

        private int panelWidth, panelHeight;
        private Camera2D camera;

        //panelWidth/panelHeight wird benötigt, damit auf Mausklicks nur reagiert wird, wenn ins untere Fenster geklickt wurde
        public SmallWindow(int panelWidth, int panelHeight, Camera2D camera)
        {
            this.panelWidth = panelWidth;
            this.panelHeight = panelHeight;
            this.camera = camera;
        }

        public void Draw(GraphicPanel2D panel, Action<Color, Color> drawLevelAction)
        {
            PhysicGlobal.BoundingBox smallWindow = new PhysicGlobal.BoundingBox(panel.Width * (1 - size), panel.Height * (1 - size), panel.Width * size, panel.Height * size);

            panel.PushMatrix();
            panel.SetTransformationMatrixToIdentity();
            panel.DisableDepthTesting();
            panel.ZValue2D = -100; //Überschreibe mit ein Wert der weit hinten liegt um somit in diesen Bildbereich den Depth-Wert zu löschen
            panel.DrawFillRectangle(backColor, (int)smallWindow.X, (int)smallWindow.Y, (int)smallWindow.GetWidth(), (int)smallWindow.GetHeight());
            panel.DrawRectangle(new Pen(Color.Blue, 3), (int)smallWindow.X, (int)smallWindow.Y, (int)smallWindow.GetWidth(), (int)smallWindow.GetHeight());

            var cameraToScreen = GetCameraToScreenMatrix(smallWindow);
            panel.MultTransformationMatrix(cameraToScreen);

            panel.EnableDepthTesting();
            drawLevelAction(frontColor, backColor);

            if (this.camera.ShowOriginalPosition == false)
            {
                panel.DisableDepthTesting();
                var cameraBox = this.camera.GetScreenBox();
                panel.DrawRectangle(new Pen(Color.Yellow, 3), (int)cameraBox.X, (int)cameraBox.Y, (int)cameraBox.GetWidth(), (int)cameraBox.GetHeight());

                //Testausgabe der Scene-Boundingbox
                //var sceneBoundingBox = this.camera.GetSceneBoundingBox().Value;
                //panel.DrawRectangle(new Pen(Color.Orange, 3), (int)sceneBoundingBox.X, (int)sceneBoundingBox.Y, (int)sceneBoundingBox.Width, (int)sceneBoundingBox.Height);
            }

            panel.PopMatrix();

        }

        private Matrix4x4 GetCameraToScreenMatrix(PhysicGlobal.BoundingBox smallWindow)
        {
            if (this.camera.ShowOriginalPosition)
            {
                var m = Matrix4x4.Scale(size, size, size);
                m *= Matrix4x4.Translate(smallWindow.X, smallWindow.Y, 0);
                return m;
            }
            else
            {
                var box = this.camera.GetSceneBoundingBox();
                float factor = Camera2D.GetScaleFactor(new SizeF(this.panelWidth, this.panelHeight), box.GetSize());
                var m = Matrix4x4.Translate(-box.X, -box.Y, 0);
                m *= Matrix4x4.Scale(size * factor, size * factor, size * factor);
                m *= Matrix4x4.Translate(smallWindow.X, smallWindow.Y, 0);

                return m;
            }


        }

        //Umrechung der Mausposition in den CameraSpace/GlobalSpace
        private Vec2D MouseToCam(Vec2D mousePosition)
        {
            PhysicGlobal.BoundingBox smallWindow = new PhysicGlobal.BoundingBox(this.panelWidth * (1 - size), this.panelHeight * (1 - size), this.panelWidth * size, this.panelHeight * size);

            var cameraToScreen = GetCameraToScreenMatrix(smallWindow);
            var screenToCamera = Matrix4x4.Invert(cameraToScreen);
            var mouseCam = Matrix4x4.MultPosition(screenToCamera, new Vector3D(mousePosition.X, mousePosition.Y, 0)).XY;

            return new Vec2D(mouseCam.X, mouseCam.Y);
        }

        public bool HandleMouseClick(MouseEventArgs e)
        {
            return IsMouseInZoomRec(e);
        }

        //Return: Handled
        public bool HandleMouseDown(MouseEventArgs e)
        {
            if (IsMouseInZoomRec(e) == false) return false;

            var mouseCam = MouseToCam(new Vec2D(e.X, e.Y));
            this.delta = new Vec2D(mouseCam.X - this.camera.X, mouseCam.Y - this.camera.Y);
            this.mouseIsDown = true;

            return true;
        }

        public bool HandleMouseMove(MouseEventArgs e)
        {
            if (this.mouseIsDown)
            {
                var mouseCam = MouseToCam(new Vec2D(e.X, e.Y));
                this.camera.X = mouseCam.X - this.delta.X;
                this.camera.Y = mouseCam.Y - this.delta.Y;

                return true;
            }

            return false;
        }

        public void HandleMouseUp(MouseEventArgs e)
        {
            this.mouseIsDown = false;
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
            if (IsMouseInZoomRec(e) == false) return;

            float size = Math.Min(1, Math.Max(-1, e.Delta / 150f)); //Clamp from -1 to +1

            var camBox1 = this.camera.GetScreenBox().GetCenter(); //Vorher
            this.camera.Zoom = Math.Max(0.1f, this.camera.Zoom * (1 + size / 10)); //Zoom darf nie 0 werden                                                                      
            var camBox2 = this.camera.GetScreenBox().GetCenter(); //Nachher

            var delta = camBox2 - camBox1;
            this.camera.X -= delta.X;
            this.camera.Y -= delta.Y;
        }

        private bool IsMouseInZoomRec(MouseEventArgs e)
        {
            var mouseCam = MouseToCam(new Vec2D(e.X, e.Y));
            var camBox = this.camera.GetScreenBox();

            return mouseCam.X > camBox.X && mouseCam.X < camBox.Max.X && mouseCam.Y > camBox.Y && mouseCam.Y < camBox.Max.Y;
        }

        public void HandleSizeChanged(int width, int height)
        {
            this.panelWidth = width;
            this.panelHeight = height;
        }
    }
}
