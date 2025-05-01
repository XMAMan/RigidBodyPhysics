using GraphicPanels;
using PhysicGlobal;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using TextureEditorGlobal;

namespace PhysicSceneDrawing
{
    internal interface ITexturedRigidBody
    {
        float ZValue { get; }
        bool IsInvisible { get; }
        PhysicGlobal.BoundingBox PhysicBoundingBox { get; }      //Weg 1: BoundingBox vom PhysicModel
        PhysicGlobal.BoundingBox TextureBoundingBox { get; }     //Weg 2: BoundingBox von den Texturdaten
        Vec2D[] GetTextureCornerPoints();        //Eckpunkte des Textur-Objektes (Rechteck oder Polygon)
        IPublicRigidBody AssociatedBody { get; }
        TextureExportData TextureExportData { get; }
        void Draw(GraphicPanel2D panel);
        void DrawPhysicBorder(GraphicPanel2D panel, Pen borderPen);
        void DrawTextureBorder(GraphicPanel2D panel, Pen borderPen);
        void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor);
    }
}
