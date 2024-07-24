using GraphicMinimal;
using GraphicPanels;

namespace LevelEditorGlobal
{
    //Hiermit können die einzelnen Starrkörper von ein PhysicLevelItem angeklickt werden, um deren CollisionCategorie oder TagData zu definieren
    public interface IMouseClickable
    {
        void Draw(GraphicPanel2D panel);
        void DrawBorder(GraphicPanel2D panel, Pen borderPen);
        bool IsPointInside(Vector2D point, Matrix4x4 screenToLocal); //modelMatrix = Mit der Matrix kann der ScreenSpace-Mauspunkt in den lokalen Objektraum gebracht werden
        Matrix4x4 GetScreenToLocalMatrix();
        float GetArea();        
    }
}
