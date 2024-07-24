using EditorControl.Model.EditorShape;
using EditorControl.ViewModel;
using GraphicPanels;
using PhysicEngine.ExportData.Thruster;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorThruster
{
    internal interface IEditorThruster
    {
        IEditorShape Body { get; }
        Color Backcolor { get; set; }
        Pen BorderPen { get; set; }
        ThrusterPropertyViewModel Properties { get; set; }
        void Draw(GraphicPanel2D panel); //Zeichnet die Editor-Daten
        IExportThruster GetExportData(List<IEditorShape> bodies);
        bool IsPointInside(Vec2D position);
        void UpdateAfterMovingBodys();
    }
}
