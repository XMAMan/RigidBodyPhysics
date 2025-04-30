using GraphicPanels;
using PhysicGlobal;
using RigidBodyPhysics.MathHelper;
using System.Drawing;
using TextureEditorGlobal;

namespace TextureEditorControl.Controls.Editor.Model.Shape
{
    class CircleShape : AreaShape, IShape
    {
        public CircleShape(I2DAreaShape circle)
            : base(circle)
        {
            this.BoundingBox = GetBoundingBox();
        }

        protected override Vec2D[] GetPhysicCornerPoints()
        {
            var c = (ICircle)this.shape;

            return new Vec2D[]
            {
                c.Center + new Vec2D(-c.Radius, -c.Radius),
                c.Center + new Vec2D(+c.Radius, -c.Radius),
                c.Center + new Vec2D(+c.Radius, +c.Radius),
                c.Center + new Vec2D(-c.Radius, +c.Radius),
            };
        }

        protected override void DrawPhysicModel(GraphicPanel2D panel, Camera2D camera)
        {
            var c = (ICircle)this.shape;

            var center = camera.PointToScreen(this.shape.Center.ToPointF()).ToGrx();
            float radius = camera.LengthToScreen(c.Radius);

            panel.DrawCircle(this.IsSelected ? new Pen(Color.Red, 4) : Pens.Black, center, radius);
        }

        public override bool IsPointInPhysicModel(Vec2D point)
        {
            var c = (ICircle)this.shape;

            return (point - c.Center).Length() <= c.Radius;
        }
    }
}
