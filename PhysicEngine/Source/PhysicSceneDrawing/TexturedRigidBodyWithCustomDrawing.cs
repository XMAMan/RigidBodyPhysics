using GraphicMinimal;
using GraphicPanels;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using TextureEditorGlobal;

namespace PhysicSceneDrawing
{
    //Dekoriert ein ITexturedRigidBody so, dass für die Zeichenmethoden ein IRigidBodyDrawer genutzt wird
    internal class TexturedRigidBodyWithCustomDrawing : ITexturedRigidBody
    {
        private ITexturedRigidBody decoree;
        private IRigidBodyDrawer bodyDrawer;
        public TexturedRigidBodyWithCustomDrawing(ITexturedRigidBody decoree, IRigidBodyDrawer bodyDrawer)
        {
            this.decoree = decoree;
            this.bodyDrawer = bodyDrawer;
        }

        public float ZValue { get => this.decoree.ZValue; }
        public bool IsInvisible { get => this.decoree.IsInvisible; }
        public RigidBodyPhysics.MathHelper.BoundingBox PhysicBoundingBox { get => this.decoree.PhysicBoundingBox; }      //Weg 1: BoundingBox vom PhysicModel
        public RigidBodyPhysics.MathHelper.BoundingBox TextureBoundingBox { get => this.decoree.TextureBoundingBox; }     //Weg 2: BoundingBox von den Texturdaten
        public Vector2D[] GetTextureCornerPoints() => this.decoree.GetTextureCornerPoints();
        public IPublicRigidBody AssociatedBody { get => this.decoree.AssociatedBody; }
        public TextureExportData TextureExportData { get => this.decoree.TextureExportData; }
        public void Draw(GraphicPanel2D panel)
        {
            this.bodyDrawer.Draw(panel);
        }
        public void DrawPhysicBorder(GraphicPanel2D panel, Pen borderPen)
        {
            this.decoree.DrawPhysicBorder(panel, borderPen);
        }
        public void DrawTextureBorder(GraphicPanel2D panel, Pen borderPen)
        {
            this.decoree.DrawTextureBorder(panel, borderPen);
        }
        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            this.bodyDrawer.DrawWithTwoColors(panel, frontColor, backColor);
        }
    }
}
