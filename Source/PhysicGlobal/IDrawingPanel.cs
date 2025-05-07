using System.Drawing;

namespace PhysicGlobal
{
    public interface IDrawingPanel
    {
        //Propertys
        int Width { get; }
        int Height { get; }
        Size Size { get; }
        float ZValue2D { get; set; }

        //Matrix-Operations
        void PushMatrix();
        void PopMatrix();
        void SetTransformationMatrixToIdentity();
        void MultTransformationMatrix(Matrix4x4 matrix);
        Matrix4x4 GetTransformationMatrix();

        //Texture-Commands
        bool IsNamedBitmapTextureAvailable(string nameWhichIsUsedForThe2DDrawingMethods);
        void CreateOrUpdateNamedBitmapTexture(string nameWhichIsUsedForThe2DDrawingMethods, Bitmap texture);
        void DrawSprite(string spriteFile, int xCount, int yCount, int xBild, int yBild, float x, float y, float width, float height, float texBorder, bool makeFirstPixelTransparent, Color colorFactor);
        Size GetTextureSize(string nameWhichIsUsedForThe2DDrawingMethods);

        //Drawing-Commands
        void DrawString(float x, float y, Color color, float size, string text);
        void DrawString(Vec2D position, Color color, float size, string text);
        Size GetStringSize(float size, string text);
        void DrawPixel(Vec2D pos, Color color, float size);
        void DrawFillRectangle(string texture, float x, float y, float width, float height, bool makeFirstPixelTransparent, Color colorFactor, float angle);
        void DrawFillRectangle(string texture, float x, float y, float width, float height, bool makeFirstPixelTransparent, Color colorFactor);
        void DrawFillRectangle(Color color, float x, float y, float width, float height, float angle);
        void DrawFillRectangle(Color color, float x, float y, float width, float height);
        void DrawRectangle(Pen pen, float x, float y, float width, float height);
        void DrawFillCircle(Color color, Vec2D pos, int radius);
        void DrawFillCircle(Color color, Vec2D pos, float radius);
        void DrawCircle(Pen pen, Vec2D pos, float radius);
        void DrawCircleWithLines(Pen pen, Vec2D pos, float radius, int pointCount);
        void DrawCircleArc(Pen pen, Vec2D pos, int radius, float startAngle, float endAngle, bool withBorderLines);
        void DrawLine(Pen pen, Vec2D p1, Vec2D p2);
        void DrawLineWithTexture(string texture, Vec2D p1, Vec2D p2, float lineWidth, bool makeFirstPixelTransparent = false);
        void DrawPolygon(Pen pen, Vec2D[] points);
        void DrawFillPolygon(Color color, Vec2D[] points);
        void DrawFillPolygon(string texture, bool makeFirstPixelTransparent, Color colorFactor, List<Vertex2D> points);
        void DrawFillRegularPolygon(Color color, Vec2D center, float radius, int cornerCount);
        void DrawFillCircleWithTriangles(Color color, Vec2D pos, float radius, int pointCount);

        //Buffer-Commands
        void ClearScreen(Color color);
        void FlipBuffer();
        Bitmap GetScreenShoot();
        int CreateFramebuffer(int width, int height, bool withColorTexture, bool withDepthTexture);
        void EnableRenderToFramebuffer(int framebufferId);
        void DisableRenderToFramebuffer();
        int GetColorTextureIdFromFramebuffer(int framebufferId);
        Bitmap GetTextureData(int textureID);

        //DepthTesting
        void EnableDepthTesting();
        void DisableDepthTesting();

        void Dispose();
    }
}
