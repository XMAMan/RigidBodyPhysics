using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;

namespace GameHelper
{
    public interface IDrawable
    {
        Rectangle GetBoundingBox();
        void Draw(GraphicPanel2D panel);
    }

    public class VoronoiExploder : ITimerHandler
    {
        private List<VoronoiPolygon> polygons;
        public VoronoiExploder(IDrawable drawable, GraphicPanel2D panel) 
        {
            var image = CreateImageFromDrawable(drawable, panel);


            string textureName = "voronoiTexture";
            panel.CreateOrUpdateNamedBitmapTexture(textureName, image);

            var voronoiCellPoints = GraphicPanel2D.GetRandomPointList(10, image.Width, image.Height, new Random());
            var voronioPolygons = GraphicPanel2D.GetVoronoiPolygons(image.Size, voronoiCellPoints);
            var box = drawable.GetBoundingBox();
            voronioPolygons = voronioPolygons.Select(x => VoronoiPolygon.TransformPolygon(x, new Vector2D(box.X, box.Y))).ToList();

            var center = new Vector2D(box.X + box.Width / 2, box.Y + box.Height / 2);
            float speed = 0.001f;
            this.polygons = voronioPolygons.Select(x => new VoronoiPolygon(textureName, x, (x[0].Position - center) * speed)).ToList();
        }

        private Bitmap CreateImageFromDrawable(IDrawable drawable, GraphicPanel2D panel)
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

        public void Draw(GraphicPanel2D panel)
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

    internal class VoronoiPolygon : ITimerHandler
    {
        private const float gravity = 0.0001f;

        private string textureName;
        private Vertex2D[] polygon;
        private Vector2D position;
        private Vector2D velocity;
        public VoronoiPolygon(string textureName, Vertex2D[] polygon, Vector2D velocity)
        {
            this.textureName = textureName;
            this.polygon = polygon;
            this.position = new Vector2D(0, 0);
            this.velocity = velocity;
        }

        public void Draw(GraphicPanel2D panel)
        {
            var movedPoly = TransformPolygon(this.polygon, this.position);
            panel.DrawFillPolygon(this.textureName, false, Color.FromArgb(255, 255, 255), movedPoly.ToList());
        }

        public void HandleTimerTick(float dt)
        {
            this.velocity.Y += gravity * dt;
            this.position += this.velocity * dt;            
        }

        internal static Vertex2D[] TransformPolygon(Vertex2D[] polygon, Vector2D position)
        {
            return polygon.Select(x => new Vertex2D(x.Position + position, x.Textcoord)).ToArray();
        }
    }
}
