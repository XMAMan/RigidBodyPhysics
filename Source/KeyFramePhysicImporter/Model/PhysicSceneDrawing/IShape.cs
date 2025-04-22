using GraphicPanels;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using WpfControls.Controls.CameraSetting;

namespace KeyFramePhysicImporter.Model.PhysicSceneDrawing
{
    internal interface IShape
    {
        IPublicRigidBody PhysicModel { get; }
        Color FillColor { get; set; }
        RigidBodyPhysics.MathHelper.BoundingBox BoundingBox { get; }
        void Draw(GraphicPanel2D panel, Pen borderPen, Color fillColor, Camera2D camera);
    }
}
