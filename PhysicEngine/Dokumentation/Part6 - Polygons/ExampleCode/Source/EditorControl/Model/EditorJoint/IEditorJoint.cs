using EditorControl.Model.EditorShape;
using GraphicPanels;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorJoint
{
    internal interface IEditorJoint
    {
        IEditorShape Body1 { get; }
        IEditorShape Body2 { get; }
        Color Backcolor { get; set; }
        Pen BorderPen { get; set; }
        bool SupportsDefineLimit { get; } //Ist es erlaubt mit der "Define Limit"-Funktion auf dieses Gelenk zu klicken?
        void UpdateAfterMovingBodys();
        void Draw(GraphicPanel2D panel); //Zeichnet die Editor-Daten
        IExportJoint GetExportData(List<IEditorShape> bodies);
        bool IsPointInside(Vec2D position);        
    }
}
