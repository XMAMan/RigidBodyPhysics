using GraphicPanels;
using PhysicGlobal;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

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
