using EditorControl.Model.EditorShape;
using EditorControl.ViewModel;
using GraphicPanels;
using PhysicEngine.ExportData.RotaryMotor;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorRotaryMotor
{
    internal interface IEditorRotaryMotor
    {
        IEditorShape Body { get; }
        Color Backcolor { get; set; }
        Pen BorderPen { get; set; }
        RotaryMotorPropertyViewModel Properties { get; set; }
        void Draw(GraphicPanel2D panel); //Zeichnet die Editor-Daten
        IExportRotaryMotor GetExportData(List<IEditorShape> bodies);
        bool IsPointInside(Vec2D position);
        void UpdateAfterMovingBodys();
    }
}
