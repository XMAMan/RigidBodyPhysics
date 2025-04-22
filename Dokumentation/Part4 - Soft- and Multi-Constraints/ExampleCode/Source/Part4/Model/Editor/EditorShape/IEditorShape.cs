using GraphicMinimal;
using GraphicPanels;
using Part4.ViewModel.Editor;
using PhysicEngine.ExportData.RigidBody;
using System.Drawing;

namespace Part4.Model.Editor.EditorShape
{
    //Mit diesen Objekt arbeitet der Editor um die Position/Ausrichtung/Masse von Starrkörpern zu definieren
    interface IEditorShape
    {
        Color Backcolor { get; set; }
        Pen BorderPen { get; set; }
        ShapePropertyViewModel Properties { get; set; }
        Vector2D Center { get; }
        float AngleInDegree { get; }
        void MoveTo(Vector2D position);
        void Rotate(float angleInDegree);
        void Resize(float size);
        void Draw(GraphicPanel2D panel); //Zeichnet die Editor-Daten
        IExportRigidBody GetExportData();
        bool IsPointInside(Vector2D position);
        BoundingBox GetBoundingBox();
    }
}
