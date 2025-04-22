using GraphicPanels;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using RigidBodyPhysics.ExportData.Joints;

namespace PhysicSceneEditorControl.Controls.Editor.Model.EditorJoint
{
    internal interface IEditorJoint : ISelectable
    {
        IEditorShape Body1 { get; }
        IEditorShape Body2 { get; }
        bool SupportsDefineLimit { get; } //Ist es erlaubt mit der "Define Limit"-Funktion auf dieses Gelenk zu klicken?
        void UpdateAfterMovingBodys();
        void Draw(GraphicPanel2D panel); //Zeichnet die Editor-Daten
        IExportJoint GetExportData(List<IEditorShape> bodies);
    }
}
