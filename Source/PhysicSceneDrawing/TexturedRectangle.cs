using GraphicMinimal;
using GraphicPanels;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using TextureEditorGlobal;

namespace PhysicSceneDrawing
{
    internal class TexturedRectangle : ITexturedRigidBody
    {
        private IPublicRigidRectangle r;
        private TextureExportData p;

        public float ZValue { get => p.ZValue; }
        public bool IsInvisible { get => p.IsInvisible; }
        
        //Weg 1: BoundingBox vom PhysicModel
        public RigidBodyPhysics.MathHelper.BoundingBox PhysicBoundingBox
        {
            get
            {
                Vec2D[] points = r.Vertex;
                return new RigidBodyPhysics.MathHelper.BoundingBox(new Vec2D(points.Min(x => x.X), points.Min(x => x.Y)),
                    new Vec2D(points.Max(x => x.X), points.Max(x => x.Y)));
            }
        }

        //Weg 2: BoundingBox von den Texturdaten
        public RigidBodyPhysics.MathHelper.BoundingBox TextureBoundingBox
        {
            get
            {
                var points = GetTextureCornerPoints();

                return RigidBodyPhysics.MathHelper.BoundingBox.GetBoxFromPoints(points.Select(x => x.ToPhx()));
            }
        }
        public Vector2D[] GetTextureCornerPoints()
        {
            float angleInDegree = r.Angle * 180 / (float)Math.PI;
            var texCenter = Vector2D.RotatePointAroundPivotPoint(r.Center.ToGrx(), r.Center.ToGrx() + new Vector2D(p.DeltaX, p.DeltaY), angleInDegree);

            Vector2D[] local = new Vector2D[]
            {
                     new Vector2D(- p.Width / 2, - p.Height / 2),
                     new Vector2D(+ p.Width / 2, - p.Height / 2),
                     new Vector2D(+ p.Width / 2, + p.Height / 2),
                     new Vector2D(- p.Width / 2, + p.Height / 2)
            };

            var points = local.Select(x => texCenter + Vector2D.RotatePointAroundPivotPoint(new Vector2D(0, 0), x, angleInDegree + p.DeltaAngle)).ToList(); //Drehe um Angle und DeltaAngle

            return points.ToArray();
        }
        public IPublicRigidBody AssociatedBody { get => r; }
        public TextureExportData TextureExportData { get => p; }
        public TexturedRectangle(IPublicRigidRectangle rectangle, TextureExportData textureData)
        {
            r = rectangle;
            p = textureData;
        }

        public void Draw(GraphicPanel2D panel)
        {
            if (string.IsNullOrEmpty(p.TextureFile))
            {
                panel.DrawPolygon(Pens.Black, r.Vertex.ToGrx().ToList());
                return;
            }

            float angleInDegree = r.Angle * 180 / (float)Math.PI;
            var texCenter = Vector2D.RotatePointAroundPivotPoint(r.Center.ToGrx(), r.Center.ToGrx() + new Vector2D(p.DeltaX, p.DeltaY), angleInDegree);

            panel.DrawFillRectangle(p.TextureFile,
                        texCenter.X, texCenter.Y,
                        p.Width, p.Height,
                        p.MakeFirstPixelTransparent,
                        p.ColorFactor,
                        angleInDegree + p.DeltaAngle);
        }

        public void DrawPhysicBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.DrawPolygon(borderPen, r.Vertex.ToGrx().ToList()); //Physik-Border
        }
        public void DrawTextureBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.DrawPolygon(borderPen, GetTextureCornerPoints().ToList()); //Texture-Border
        }
        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            panel.DrawFillPolygon(frontColor, r.Vertex.ToGrx().ToList());
        }
    }
}
