using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model
{
    internal interface ISelectable
    {
        Color Backcolor { get; set; }   //Ändert sich beim MouseOver-Effekt
        Pen BorderPen { get; set; }     //Ändert sich beim Selektieren
        bool IsPointInside(Vec2D position); //Zur Abfrage, ob die Maus drüber ist
    }

    internal interface ISelectableShape : ISelectable
    {
        float GetArea(); //Wenn die Maus über mehreren Flächen ist, dann gib nur die kleinste Fläche zurück
    }
}
