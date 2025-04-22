using GraphicMinimal;
using GraphicPanels;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using TextureEditorGlobal;

namespace PhysicSceneDrawing
{
    internal class TexturedPolygon : ITexturedRigidBody
    {
        private IPublicRigidPolygon r;
        private TextureExportData p;
        private Vector2D[] uvCoords;
        public float ZValue { get => p.ZValue; }
        public bool IsInvisible { get => p.IsInvisible; }
        public RigidBodyPhysics.MathHelper.BoundingBox PhysicBoundingBox
        {
            get
            {
                Vec2D[] points = r.Vertex;
                return new RigidBodyPhysics.MathHelper.BoundingBox(new Vec2D(points.Min(x => x.X), points.Min(x => x.Y)),
                    new Vec2D(points.Max(x => x.X), points.Max(x => x.Y)));
            }
        }
        public RigidBodyPhysics.MathHelper.BoundingBox TextureBoundingBox
        {
            get
            {
                return PhysicBoundingBox;
            }
        }
        public Vector2D[] GetTextureCornerPoints()
        {
            return GetTextureBorderPoints();
            //return r.Vertex.Select(x => x.ToGrx()).ToArray();
        }
        public IPublicRigidBody AssociatedBody { get => r; }
        public TextureExportData TextureExportData { get => p; }
        public TexturedPolygon(IPublicRigidPolygon polygon, TextureExportData textureData)
        {
            r = polygon;
            p = textureData;

            uvCoords = GetUVCoords();
        }

        private Vector2D[] GetUVCoords()
        {
            //Texturrechteck, was um Delta verschoben wurde und dann noch Polygon.AngleInDegree gedreht wurde
            var texPoints = GetTextureBorderPoints();

            float xLength = (texPoints[1] - texPoints[0]).Length();
            float yLength = (texPoints[3] - texPoints[0]).Length();
            var xDir = (texPoints[1] - texPoints[0]) / xLength;
            var yDir = (texPoints[3] - texPoints[0]) / yLength;

            float angleInDegree = r.Angle / (float)Math.PI * 180;
            var exportData = (PolygonExportData)r.GetExportData();
            var globalPoints = exportData.Points
                .Select(x => exportData.Center + x)
                .Select(x => Vec2D.RotatePointAroundPivotPoint(exportData.Center, x, angleInDegree))
                .ToGrx().ToList();

            List<Vector2D> coords = new List<Vector2D>();
            foreach (var local in globalPoints)
            {
                var tex = new Vector2D((local - texPoints[0]) * xDir / xLength, (local - texPoints[0]) * yDir / yLength);
                coords.Add(tex);
            }

            return coords.ToArray();
        }

        protected Vector2D[] GetTextureBorderPoints()
        {
            float angleInDegree = r.Angle / (float)Math.PI * 180;
            var localCenter = RigidBodyPhysics.MathHelper.BoundingBox.GetBoxFromPoints(((PolygonExportData)r.GetExportData()).Points).Center.ToGrx();
            return TextureRectangleHelper.GetTextureBorderPoints(r.Center.ToGrx(), localCenter, p.Width, p.Height, angleInDegree, p.DeltaX, p.DeltaY, p.DeltaAngle);
        }

        public void Draw(GraphicPanel2D panel)
        {
            if (string.IsNullOrEmpty(p.TextureFile))
            {
                panel.DrawPolygon(Pens.Black, r.Vertex.ToGrx().ToList());
                return;
            }

            List<Vertex2D> points = new List<Vertex2D>();
            for (int i = 0; i < uvCoords.Length; i++)
            {
                points.Add(new Vertex2D(r.Vertex[i].ToGrx(), uvCoords[i]));
            }

            panel.DrawFillPolygon(p.TextureFile, p.MakeFirstPixelTransparent, p.ColorFactor, points.ToList());
        }

        public void DrawPhysicBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.DrawPolygon(borderPen, r.Vertex.ToGrx().ToList()); //Physik-Border
        }
        public void DrawTextureBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.DrawPolygon(borderPen, GetTextureCornerPoints().Take(4).ToList()); //Texture-Border
        }

        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            var color = r.PolygonType == PolygonCollisionType.EdgeWithNormalsPointingInside ? backColor : frontColor;
            panel.DrawFillPolygon(color, r.Vertex.ToGrx().ToList());
        }
    }
}
