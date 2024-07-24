using GraphicPanels;
using PhysicSceneEditorControl.Controls.ShapeProperty;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.EditorShape
{
    //Mit diesen Objekt arbeitet der Editor um die Position/Ausrichtung/Masse von Starrkörpern zu definieren
    interface IEditorShape : ISelectableShape
    {
        ShapePropertyViewModel Properties { get; set; }
        Vec2D Center { get; }
        float AngleInDegree { get; }
        void MoveTo(Vec2D position);
        void Rotate(float angleInDegree);
        void Resize(float size);
        void Draw(GraphicPanel2D panel); //Zeichnet die Editor-Daten
        IExportRigidBody GetExportData();
        BoundingBox GetBoundingBox();
        Vec2D[] GetAnchorPoints();
    }
}
