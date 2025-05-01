using PhysicGlobal;
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

        protected override Vec2D[] GetPhysicCornerPoints()
        {
            var r = (IRectangle)this.shape;
            float angleInDegree = r.AngleInDegree;
            return new Vec2D[]
                        {
                            Vec2D.RotatePointAroundPivotPoint(r.Center, new Vec2D(r.Center.X + r.Width / 2, r.Center.Y + r.Height / 2), angleInDegree),
                            Vec2D.RotatePointAroundPivotPoint(r.Center, new Vec2D(r.Center.X - r.Width / 2, r.Center.Y + r.Height / 2), angleInDegree),
                            Vec2D.RotatePointAroundPivotPoint(r.Center, new Vec2D(r.Center.X - r.Width / 2, r.Center.Y - r.Height / 2), angleInDegree),
                            Vec2D.RotatePointAroundPivotPoint(r.Center, new Vec2D(r.Center.X + r.Width / 2, r.Center.Y - r.Height / 2), angleInDegree),
                        };
        }

        public override bool IsPointInPhysicModel(Vec2D point)
        {
            return MathHelper.IsPointInRectangle(GetPhysicCornerPoints(), point);
        }
    }
}
