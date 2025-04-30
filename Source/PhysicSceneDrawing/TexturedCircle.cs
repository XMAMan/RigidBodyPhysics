using GraphicMinimal;
using GraphicPanels;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using TextureEditorGlobal;

namespace PhysicSceneDrawing
{
    internal class TexturedCircle : ITexturedRigidBody
    {
        private IPublicRigidCircle r;
        private TextureExportData p;
        public float ZValue { get => p.ZValue; }
        public bool IsInvisible { get => p.IsInvisible; }
        public RigidBodyPhysics.MathHelper.BoundingBox PhysicBoundingBox
        {
            get
            {
                return new RigidBodyPhysics.MathHelper.BoundingBox(new Vec2D(r.Center.X - r.Radius, r.Center.Y - r.Radius),
                    new Vec2D(r.Center.X + r.Radius, r.Center.Y + r.Radius));
            }
        }
        public RigidBodyPhysics.MathHelper.BoundingBox TextureBoundingBox
        {
            get
            {
                var points = GetTextureCornerPoints();

                return RigidBodyPhysics.MathHelper.BoundingBox.GetBoxFromPoints(points);
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
        public TexturedCircle(IPublicRigidCircle circle, TextureExportData textureData)
        {
            r = circle;
            p = textureData;
        }

        public void Draw(GraphicPanel2D panel)
        {
            if (string.IsNullOrEmpty(p.TextureFile))
            {
                if (r.Radius > 1)
                    panel.DrawCircle(Pens.Black, r.Center.ToGrx(), r.Radius);   //Physik-Border
                else
                    panel.DrawCircleWithLines(Pens.Black, r.Center.ToGrx(), r.Radius, 10);

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
            if (r.Radius > 1)
                panel.DrawCircle(borderPen, r.Center.ToGrx(), r.Radius);   //Physik-Border
            else
                panel.DrawCircleWithLines(borderPen, r.Center.ToGrx(), r.Radius, 10);            
        }
        public void DrawTextureBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.DrawPolygon(borderPen, GetTextureCornerPoints().ToGrx().ToList()); //Texture-Border
        }

        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            if (r.Radius > 1)
                panel.DrawFillCircle(frontColor, r.Center.ToGrx(), r.Radius);
            else
                panel.DrawFillCircleWithTriangles(frontColor, r.Center.ToGrx(), r.Radius, 10);
        }
    }
}
