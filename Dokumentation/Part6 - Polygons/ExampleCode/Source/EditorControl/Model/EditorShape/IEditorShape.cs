using EditorControl.ViewModel;
using GraphicPanels;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorShape
{
    //Mit diesen Objekt arbeitet der Editor um die Position/Ausrichtung/Masse von Starrkörpern zu definieren
    interface IEditorShape
    {
        Color Backcolor { get; set; }
        Pen BorderPen { get; set; }
        ShapePropertyViewModel Properties { get; set; }
        Vec2D Center { get; }
        float AngleInDegree { get; }
        float GetArea();
        void MoveTo(Vec2D position);
        void Rotate(float angleInDegree);
        void Resize(float size);
        void Draw(GraphicPanel2D panel); //Zeichnet die Editor-Daten
        IExportRigidBody GetExportData();
        bool IsPointInside(Vec2D position);
        BoundingBox GetBoundingBox();
        Vec2D[] GetAnchorPoints();
    }
}
