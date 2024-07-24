using GraphicPanels;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using PhysicSceneEditorControl.Controls.RotaryMotorProperty;
using RigidBodyPhysics.ExportData.RotaryMotor;

namespace PhysicSceneEditorControl.Controls.Editor.Model.EditorRotaryMotor
{
    internal interface IEditorRotaryMotor : ISelectable
    {
        IEditorShape Body { get; }
        RotaryMotorPropertyViewModel Properties { get; set; }
        void Draw(GraphicPanel2D panel); //Zeichnet die Editor-Daten
        IExportRotaryMotor GetExportData(List<IEditorShape> bodies);
        void UpdateAfterMovingBodys();
    }
}
