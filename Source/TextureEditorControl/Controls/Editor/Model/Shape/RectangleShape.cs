using GraphicMinimal;
using TextureEditorGlobal;

namespace TextureEditorControl.Controls.Editor.Model.Shape
{
    class RectangleShape : AreaShape, IShape
    {
        public RectangleShape(I2DAreaShape circle)
            : base(circle)
        {
            this.BoundingBox = GetBoundingBox();
        }

        protected override Vector2D[] GetPhysicCornerPoints()
        {
            var r = (IRectangle)this.shape;
            float angleInDegree = r.AngleInDegree;
            return new Vector2D[]
                        {
                            Vector2D.RotatePointAroundPivotPoint(r.Center, new Vector2D(r.Center.X + r.Width / 2, r.Center.Y + r.Height / 2), angleInDegree),
                            Vector2D.RotatePointAroundPivotPoint(r.Center, new Vector2D(r.Center.X - r.Width / 2, r.Center.Y + r.Height / 2), angleInDegree),
                            Vector2D.RotatePointAroundPivotPoint(r.Center, new Vector2D(r.Center.X - r.Width / 2, r.Center.Y - r.Height / 2), angleInDegree),
                            Vector2D.RotatePointAroundPivotPoint(r.Center, new Vector2D(r.Center.X + r.Width / 2, r.Center.Y - r.Height / 2), angleInDegree),
                        };
        }

        public override bool IsPointInPhysicModel(Vector2D point)
        {
            return MathHelper.IsPointInRectangle(GetPhysicCornerPoints(), point);
        }
    }
}
