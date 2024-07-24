using GraphicMinimal;
using GraphicPanels;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using TextureEditorGlobal;

namespace PhysicSceneDrawing
{
    internal interface ITexturedRigidBody
    {
        float ZValue { get; }
        bool IsInvisible { get; }
        RigidBodyPhysics.MathHelper.BoundingBox PhysicBoundingBox { get; }      //Weg 1: BoundingBox vom PhysicModel
        RigidBodyPhysics.MathHelper.BoundingBox TextureBoundingBox { get; }     //Weg 2: BoundingBox von den Texturdaten
        Vector2D[] GetTextureCornerPoints();        //Eckpunkte des Textur-Objektes (Rechteck oder Polygon)
        IPublicRigidBody AssociatedBody { get; }
        TextureExportData TextureExportData { get; }
        void Draw(GraphicPanel2D panel);
        void DrawPhysicBorder(GraphicPanel2D panel, Pen borderPen);
        void DrawTextureBorder(GraphicPanel2D panel, Pen borderPen);
        void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor);
    }
}
