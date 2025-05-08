using GraphicPanels;
using PhysicGlobal;
using System.Runtime.InteropServices;

namespace DrawingPanel
{
    public class DrawingPanel : PhysicGlobal.IDrawingPanel
    {
        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern void GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS awareness);

        private enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware = 0,            //Bild ist um 150% größer. Beispiel: Mein Bildviewer
            Process_System_DPI_Aware = 1,       //Bild ist ganz klein. Beispielanwendung: Paint
            Process_Per_Monitor_DPI_Aware = 2
        }


        private GraphicPanel2D panel;

        //Nutze diese Property, wenn die Zeichenoperationen vom IDrawingPanel-Interface nicht reichen
        public GraphicPanel2D Panel { get => this.panel; } 

        public new event MouseEventHandler MouseClick
        {
            add
            {
                this.panel.MouseClick += value;
            }
            remove
            {
                this.panel.MouseClick -= value;
            }
        }

        public new event MouseEventHandler MouseWheel
        {
            add
            {
                this.panel.MouseWheel += value;
            }
            remove
            {
                this.panel.MouseWheel -= value;
            }
        }

        public new event MouseEventHandler MouseMove
        {
            add
            {
                this.panel.MouseMove += value;
            }
            remove
            {
                this.panel.MouseMove -= value;
            }
        }

        public new event MouseEventHandler MouseDown
        {
            add
            {
                this.panel.MouseDown += value;
            }
            remove
            {
                this.panel.MouseDown -= value;
            }
        }

        public new event MouseEventHandler MouseUp
        {
            add
            {
                this.panel.MouseUp += value;
            }
            remove
            {
                this.panel.MouseUp -= value;
            }
        }

        public new event EventHandler SizeChanged
        {
            add
            {
                this.panel.SizeChanged += value;
            }
            remove
            {
                this.panel.SizeChanged -= value;
            }
        }

        public new event EventHandler MouseEnter
        {
            add
            {
                this.panel.MouseEnter += value;
            }
            remove
            {
                this.panel.MouseEnter -= value;
            }
        }

        public new event EventHandler MouseLeave
        {
            add
            {
                this.panel.MouseLeave += value;
            }
            remove
            {
                this.panel.MouseLeave -= value;
            }
        }

        public DrawingPanel(GraphicPanel2D panel)
        {
            this.panel = panel;
        }

        public DrawingPanel(int width, int height, bool useCpuMode = false)
        {
            _ = SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_DPI_Unaware); //Damit ich unter Windows 10 kein kleines OpenGL3.0-Fenster erhalte
            this.panel = new GraphicPanel2D() { Width = width, Height = height, Mode = useCpuMode ? Mode2D.CPU : Mode2D.OpenGL_Version_3_0 };
        }

        //Propertys
        public int Width { get => this.panel.Width; }
        public int Height { get => this.panel.Height; }
        public Size Size { get => this.panel.Size; }
        public float ZValue2D { get => this.panel.ZValue2D; set => this.panel.ZValue2D = value; }

        //Matrix-Operations
        public void PushMatrix() => this.panel.PushMatrix();
        public void PopMatrix() => this.panel.PopMatrix();
        public void SetTransformationMatrixToIdentity() => this.panel.SetTransformationMatrixToIdentity();
        public void MultTransformationMatrix(Matrix4x4 matrix) => this.panel.MultTransformationMatrix(matrix.To4x4Matrix());
        public Matrix4x4 GetTransformationMatrix() => this.panel.GetTransformationMatrix().ToPhxMatrix();

        //Texture-Commands
        public bool IsNamedBitmapTextureAvailable(string nameWhichIsUsedForThe2DDrawingMethods) 
            => this.panel.IsNamedBitmapTextureAvailable(nameWhichIsUsedForThe2DDrawingMethods);
        public void CreateOrUpdateNamedBitmapTexture(string nameWhichIsUsedForThe2DDrawingMethods, Bitmap texture)
            => this.panel.CreateOrUpdateNamedBitmapTexture(nameWhichIsUsedForThe2DDrawingMethods, texture);
        public void DrawSprite(string spriteFile, int xCount, int yCount, int xBild, int yBild, float x, float y, float width, float height, float texBorder, bool makeFirstPixelTransparent, Color colorFactor)
            => this.panel.DrawSprite(spriteFile, xCount, yCount, xBild, yBild, x, y, width, height, texBorder, makeFirstPixelTransparent, colorFactor);
        public Size GetTextureSize(string nameWhichIsUsedForThe2DDrawingMethods)
            => this.panel.GetTextureSize(nameWhichIsUsedForThe2DDrawingMethods);

        //Drawing-Commands
        public void DrawString(float x, float y, Color color, float size, string text) 
            => this.panel.DrawString(x, y, color, size, text);
        public void DrawString(Vec2D position, Color color, float size, string text)
            => this.panel.DrawString(position.ToGrx(), color, size, text);
        public Size GetStringSize(float size, string text)
            => this.panel.GetStringSize(size, text);
        public void DrawPixel(Vec2D pos, Color color, float size)
            => this.panel.DrawPixel(pos.ToGrx(), color, size);
        public void DrawFillRectangle(string texture, float x, float y, float width, float height, bool makeFirstPixelTransparent, Color colorFactor, float angle)
            => this.panel.DrawFillRectangle(texture, x, y, width, height, makeFirstPixelTransparent, colorFactor, angle);
        public void DrawFillRectangle(string texture, float x, float y, float width, float height, bool makeFirstPixelTransparent, Color colorFactor)
            => this.panel.DrawFillRectangle(texture, x, y, width, height, makeFirstPixelTransparent, colorFactor);
        public void DrawFillRectangle(Color color, float x, float y, float width, float height, float angle)
            => this.panel.DrawFillRectangle(color, x, y, width, height, angle);
        public void DrawFillRectangle(Color color, float x, float y, float width, float height)
            => this.panel.DrawFillRectangle(color, x, y, width, height);
        public void DrawRectangle(Pen pen, float x, float y, float width, float height)
            => this.panel.DrawRectangle(pen, x, y, width, height);
        public void DrawFillCircle(Color color, Vec2D pos, int radius)
            => this.panel.DrawFillCircle(color, pos.ToGrx(), radius);
        public void DrawFillCircle(Color color, Vec2D pos, float radius)
            => this.panel.DrawFillCircle(color, pos.ToGrx(), radius);
        public void DrawCircle(Pen pen, Vec2D pos, float radius)
            => this.panel.DrawCircle(pen, pos.ToGrx(), radius);
        public void DrawCircleWithLines(Pen pen, Vec2D pos, float radius, int pointCount)
            => this.panel.DrawCircleWithLines(pen, pos.ToGrx(), radius, pointCount);
        public void DrawCircleArc(Pen pen, Vec2D pos, int radius, float startAngle, float endAngle, bool withBorderLines)
            => this.panel.DrawCircleArc(pen, pos.ToGrx(), radius, startAngle, endAngle, withBorderLines);
        public void DrawLine(Pen pen, Vec2D p1, Vec2D p2)
            => this.panel.DrawLine(pen, p1.ToGrx(), p2.ToGrx());
        public void DrawLineWithTexture(string texture, Vec2D p1, Vec2D p2, float lineWidth, bool makeFirstPixelTransparent = false)
            => this.panel.DrawLineWithTexture(texture, p1.ToGrx(), p2.ToGrx(), lineWidth, makeFirstPixelTransparent);
        public void DrawPolygon(Pen pen, Vec2D[] points)
            => this.panel.DrawPolygon(pen, points.Select(x => x.ToGrx()).ToList());
        public void DrawFillPolygon(Color color, Vec2D[] points)
            => this.panel.DrawFillPolygon(color, points.Select(x => x.ToGrx()).ToList());
        public void DrawFillPolygon(string texture, bool makeFirstPixelTransparent, Color colorFactor, List<PhysicGlobal.Vertex2D> points)
            => this.panel.DrawFillPolygon(texture, makeFirstPixelTransparent, colorFactor, points.Select(x => new GraphicMinimal.Vertex2D(x.Position.ToGrx(), x.Textcoord.ToGrx())).ToList());
        public void DrawFillRegularPolygon(Color color, Vec2D center, float radius, int cornerCount)
            => this.panel.DrawFillRegularPolygon(color, center.ToGrx(), radius, cornerCount);
        public void DrawFillCircleWithTriangles(Color color, Vec2D pos, float radius, int pointCount)
            => this.panel.DrawFillCircleWithTriangles(color, pos.ToGrx(), radius, pointCount);

        //Buffer-Commands
        public void ClearScreen(Color color)
            => this.panel.ClearScreen(color);
        public void FlipBuffer()
            => this.panel.FlipBuffer();
        public Bitmap GetScreenShoot()
            => this.panel.GetScreenShoot();
        public int CreateFramebuffer(int width, int height, bool withColorTexture, bool withDepthTexture)
            => this.panel.CreateFramebuffer(width, height, withColorTexture, withDepthTexture);
        public void EnableRenderToFramebuffer(int framebufferId)
            => this.panel.EnableRenderToFramebuffer(framebufferId);
        public void DisableRenderToFramebuffer()
            => this.panel.DisableRenderToFramebuffer();
        public int GetColorTextureIdFromFramebuffer(int framebufferId)
            => this.panel.GetColorTextureIdFromFramebuffer(framebufferId);
        public Bitmap GetTextureData(int textureID)
            => this.panel.GetTextureData(textureID);

        //DepthTesting
        public void EnableDepthTesting()
            => this.panel.EnableDepthTesting();
        public void DisableDepthTesting()
            => this.panel.DisableDepthTesting();

        #region IDisposable
        public void Dispose()
        {
            this.panel.Dispose();
            this.panel = null;
        }
        #endregion
    }
}
