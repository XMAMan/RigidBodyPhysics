using PhysicGlobal;
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
        public PhysicGlobal.BoundingBox PhysicBoundingBox
        {
            get
            {
                Vec2D[] points = r.Vertex;
                return new PhysicGlobal.BoundingBox(new Vec2D(points.Min(x => x.X), points.Min(x => x.Y)),
                    new Vec2D(points.Max(x => x.X), points.Max(x => x.Y)));
            }
        }

        //Weg 2: BoundingBox von den Texturdaten
        public PhysicGlobal.BoundingBox TextureBoundingBox
        {
            get
            {
                var points = GetTextureCornerPoints();

                return PhysicGlobal.BoundingBox.GetBoxFromPoints(points);
            }
        }
        public Vec2D[] GetTextureCornerPoints()
        {
            float angleInDegree = r.Angle * 180 / (float)Math.PI;
            var texCenter = Vec2D.RotatePointAroundPivotPoint(r.Center, r.Center + new Vec2D(p.DeltaX, p.DeltaY), angleInDegree);

            Vec2D[] local = new Vec2D[]
            {
                     new Vec2D(- p.Width / 2, - p.Height / 2),
                     new Vec2D(+ p.Width / 2, - p.Height / 2),
                     new Vec2D(+ p.Width / 2, + p.Height / 2),
                     new Vec2D(- p.Width / 2, + p.Height / 2)
            };

            var points = local.Select(x => texCenter + Vec2D.RotatePointAroundPivotPoint(new Vec2D(0, 0), x, angleInDegree + p.DeltaAngle)).ToList(); //Drehe um Angle und DeltaAngle

            return points.ToArray();
        }
        public IPublicRigidBody AssociatedBody { get => r; }
        public TextureExportData TextureExportData { get => p; }
        public TexturedRectangle(IPublicRigidRectangle rectangle, TextureExportData textureData)
        {
            r = rectangle;
            p = textureData;
        }

        public void Draw(IDrawingPanel panel)
        {
            if (string.IsNullOrEmpty(p.TextureFile))
            {
                panel.DrawPolygon(Pens.Black, r.Vertex);
                return;
            }

            float angleInDegree = r.Angle * 180 / (float)Math.PI;
            var texCenter = Vec2D.RotatePointAroundPivotPoint(r.Center, r.Center + new Vec2D(p.DeltaX, p.DeltaY), angleInDegree);

            panel.DrawFillRectangle(p.TextureFile,
                        texCenter.X, texCenter.Y,
                        p.Width, p.Height,
                        p.MakeFirstPixelTransparent,
                        p.ColorFactor,
                        angleInDegree + p.DeltaAngle);
        }

        public void DrawPhysicBorder(IDrawingPanel panel, Pen borderPen)
        {
            panel.DrawPolygon(borderPen, r.Vertex); //Physik-Border
        }
        public void DrawTextureBorder(IDrawingPanel panel, Pen borderPen)
        {
            panel.DrawPolygon(borderPen, GetTextureCornerPoints().ToArray()); //Texture-Border
        }
        public void DrawWithTwoColors(IDrawingPanel panel, Color frontColor, Color backColor)
        {
            panel.DrawFillPolygon(frontColor, r.Vertex);
        }
    }
}
