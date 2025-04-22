using GraphicPanels;
using PhysicSceneEditorControl.Controls.AxialFriction;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using RigidBodyPhysics.ExportData.AxialFriction;

namespace PhysicSceneEditorControl.Controls.Editor.Model.EditorAxialFriction
{
    internal interface IEditorAxialFriction : ISelectable
    {
        IEditorShape Body { get; }        
        AxialFrictionPropertyViewModel Properties { get; set; }
        void Draw(GraphicPanel2D panel); //Zeichnet die Editor-Daten
        IExportAxialFriction GetExportData(List<IEditorShape> bodies);
        void UpdateAfterMovingBodys();
    }
}
