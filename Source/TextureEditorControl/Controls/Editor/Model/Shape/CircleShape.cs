using GraphicMinimal;
using GraphicPanels;
using System.Drawing;
using TextureEditorGlobal;
using WpfControls.Controls.CameraSetting;

namespace TextureEditorControl.Controls.Editor.Model.Shape
{
    class CircleShape : AreaShape, IShape
    {
        public CircleShape(I2DAreaShape circle)
            : base(circle)
        {
            this.BoundingBox = GetBoundingBox();
        }

        protected override Vector2D[] GetPhysicCornerPoints()
        {
            var c = (ICircle)this.shape;

            return new Vector2D[]
            {
                c.Center + new Vector2D(-c.Radius, -c.Radius),
                c.Center + new Vector2D(+c.Radius, -c.Radius),
                c.Center + new Vector2D(+c.Radius, +c.Radius),
                c.Center + new Vector2D(-c.Radius, +c.Radius),
            };
        }

        protected override void DrawPhysicModel(GraphicPanel2D panel, Camera2D camera)
        {
            var c = (ICircle)this.shape;

            var center = camera.PointToScreen(this.shape.Center.ToPointF()).ToGrx();
            float radius = camera.LengthToScreen(c.Radius);

            panel.DrawCircle(this.IsSelected ? new Pen(Color.Red, 4) : Pens.Black, center, radius);
        }

        public override bool IsPointInPhysicModel(Vector2D point)
        {
            var c = (ICircle)this.shape;

            return (point - c.Center).Length() <= c.Radius;
        }
    }
}
