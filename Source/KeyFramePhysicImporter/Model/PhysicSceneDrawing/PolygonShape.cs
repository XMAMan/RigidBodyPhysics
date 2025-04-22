using GraphicPanels;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using WpfControls.Controls.CameraSetting;

namespace KeyFramePhysicImporter.Model.PhysicSceneDrawing
{
    internal class PolygonShape : IShape
    {
        private IPublicRigidPolygon model;
        public PolygonShape(IPublicRigidPolygon ctor)
        {
            this.PhysicModel = this.model = ctor;
        }
        public IPublicRigidBody PhysicModel { get; }
        public Color FillColor { get; set; } = Color.Transparent;
        public BoundingBox BoundingBox
        {
            get
            {
                Vec2D[] points = model.Vertex;
                return new BoundingBox(new Vec2D(points.Min(x => x.X), points.Min(x => x.Y)),
                    new Vec2D(points.Max(x => x.X), points.Max(x => x.Y)));
            }
        }

        public void Draw(GraphicPanel2D panel, Pen borderPen, Color fillColor, Camera2D camera)
        {
            var points = this.model.Vertex.Select(x => camera.PointToScreen(x.ToPointF()).ToGrx()).ToList();
            panel.DrawFillPolygon(fillColor, points);
            panel.DrawPolygon(borderPen, points);
        }
    }
}
