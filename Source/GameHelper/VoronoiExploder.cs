using DynamicObjCreation.RigidBodyDestroying;
using PhysicGlobal;

namespace GameHelper
{
    public interface IDrawable
    {
        Rectangle GetBoundingBox();
        void Draw(IDrawingPanel panel);
    }

    public class VoronoiExploder
    {
        private List<VoronoiPolygon> polygons;
        public VoronoiExploder(IDrawable drawable, IDrawingPanel panel) 
        {
            var image = CreateImageFromDrawable(drawable, panel);


            string textureName = "voronoiTexture";
            panel.CreateOrUpdateNamedBitmapTexture(textureName, image);

            var voronoiCellPoints = VoronoiHelper.GetRandomPointList(10, image.Width, image.Height, new Random());
            var voronioPolygons = VoronoiHelper.GetVoronoiPolygons(image.Size, voronoiCellPoints);
            var box = drawable.GetBoundingBox();
            voronioPolygons = voronioPolygons.Select(x => VoronoiPolygon.TransformPolygon(x, new Vec2D(box.X, box.Y))).ToList();

            var center = new Vec2D(box.X + box.Width / 2, box.Y + box.Height / 2);
            float speed = 0.001f;
            this.polygons = voronioPolygons.Select(x => new VoronoiPolygon(textureName, x, (x[0].Position - center) * speed)).ToList();
        }

        private Bitmap CreateImageFromDrawable(IDrawable drawable, IDrawingPanel panel)
        {
            var box = drawable.GetBoundingBox();

            int frameBufferId = panel.CreateFramebuffer(box.Width, box.Height, true, false);
            panel.EnableRenderToFramebuffer(frameBufferId);
            panel.ClearScreen(Color.Transparent);
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-box.X, -box.Y, 0));
            drawable.Draw(panel);

            panel.PopMatrix();
            panel.FlipBuffer();

            int colorTextureId = panel.GetColorTextureIdFromFramebuffer(frameBufferId);
            Bitmap image = panel.GetTextureData(colorTextureId);
            panel.DisableRenderToFramebuffer();

            return image;
        }

        public void Draw(IDrawingPanel panel)
        {
            foreach (var poly in this.polygons)
            {
                poly.Draw(panel);
            }
        }

        public void HandleTimerTick(float dt)
        {
            foreach (var poly in this.polygons)
            {
                poly.HandleTimerTick(dt);
            }
        }
    }

    internal class VoronoiPolygon
    {
        private const float gravity = 0.0001f;

        private string textureName;
        private Vertex2D[] polygon;
        private Vec2D position;
        private Vec2D velocity;
        public VoronoiPolygon(string textureName, Vertex2D[] polygon, Vec2D velocity)
        {
            this.textureName = textureName;
            this.polygon = polygon;
            this.position = new Vec2D(0, 0);
            this.velocity = velocity;
        }

        public void Draw(IDrawingPanel panel)
        {
            var movedPoly = TransformPolygon(this.polygon, this.position);
            panel.DrawFillPolygon(this.textureName, false, Color.FromArgb(255, 255, 255), movedPoly.ToList());
        }

        public void HandleTimerTick(float dt)
        {
            this.velocity.Y += gravity * dt;
            this.position += this.velocity * dt;            
        }

        internal static Vertex2D[] TransformPolygon(Vertex2D[] polygon, Vec2D position)
        {
            return polygon.Select(x => new Vertex2D(x.Position + position, x.Textcoord)).ToArray();
        }
    }
}
