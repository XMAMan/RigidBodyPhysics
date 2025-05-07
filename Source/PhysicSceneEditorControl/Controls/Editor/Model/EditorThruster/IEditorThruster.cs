using PhysicGlobal;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using PhysicSceneEditorControl.Controls.ThrusterProperty;
using RigidBodyPhysics.ExportData.Thruster;

namespace PhysicSceneEditorControl.Controls.Editor.Model.EditorThruster
{
    internal interface IEditorThruster : ISelectable
    {
        IEditorShape Body { get; }
        ThrusterPropertyViewModel Properties { get; set; }
        void Draw(IDrawingPanel panel); //Zeichnet die Editor-Daten
        IExportThruster GetExportData(List<IEditorShape> bodies);
        void UpdateAfterMovingBodys();
    }
}
