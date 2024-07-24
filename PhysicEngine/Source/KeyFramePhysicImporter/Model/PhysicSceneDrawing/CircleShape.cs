using GraphicPanels;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using WpfControls.Controls.CameraSetting;

namespace KeyFramePhysicImporter.Model.PhysicSceneDrawing
{
    internal class CircleShape : IShape
    {
        private IPublicRigidCircle model;
        public CircleShape(IPublicRigidCircle ctor)
        {
            this.PhysicModel = this.model = ctor;
        }

        public IPublicRigidBody PhysicModel { get; }
        public Color FillColor { get; set; } = Color.Transparent;
        public BoundingBox BoundingBox
        {
            get
            {
                return new BoundingBox(new Vec2D(model.Center.X - model.Radius, model.Center.Y - model.Radius),
                    new Vec2D(model.Center.X + model.Radius, model.Center.Y + model.Radius));
            }
        }

        public void Draw(GraphicPanel2D panel, Pen borderPen, Color fillColor, Camera2D camera)
        {
            var pos = camera.PointToScreen(this.model.Center.ToPointF()).ToGrx();
            float radius = camera.LengthToScreen(this.model.Radius);
            panel.DrawFillCircle(fillColor, pos, radius);
            panel.DrawCircle(borderPen, pos, radius);
        }
    }
}
