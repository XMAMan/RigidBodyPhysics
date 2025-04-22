using GraphicMinimal;
using GraphicPanels;
using Part2.ViewModel.Editor;
using PhysicEngine.ExportData;
using System.Drawing;

namespace Part2.Model.Editor.EditorShape
{
    //Mit diesen Objekt arbeitet der Editor um die Position/Ausrichtung/Masse von Starrkörpern zu definieren
    interface IEditorShape
    {
        Color Backcolor { get; set; }
        Pen BorderPen { get; set; }
        ShapePropertyViewModel Properties { get; set; }
        Vector2D Center { get; }
        void MoveTo(Vector2D position);
        void Rotate(float angleInDegree);
        void Resize(float size);
        void Draw(GraphicPanel2D panel); //Zeichnet die Editor-Daten
        IExportShape GetExportData();
        bool IsPointInside(Vector2D position);        
    }
}
