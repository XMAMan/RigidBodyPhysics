using GraphicMinimal;
using GraphicPanels;
using Part4.Model.Editor.EditorShape;
using PhysicEngine.ExportData.Joints;
using System.Collections.Generic;
using System.Drawing;

namespace Part4.Model.Editor.EditorJoint
{
    internal interface IEditorJoint
    {
        IEditorShape Body1 { get; }
        IEditorShape Body2 { get; }
        Color Backcolor { get; set; }
        Pen BorderPen { get; set; }
        Vector2D Center { get; }
        void MoveTo(Vector2D position);
        void UpdateAfterMovingBodys();
        void Draw(GraphicPanel2D panel); //Zeichnet die Editor-Daten
        IExportJoint GetExportData(List<IEditorShape> bodies);
        bool IsPointInside(Vector2D position);
    }
}
