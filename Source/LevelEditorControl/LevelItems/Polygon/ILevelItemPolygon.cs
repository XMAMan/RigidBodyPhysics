using PhysicGlobal;

namespace LevelEditorControl.LevelItems.Polygon
{
    internal interface ILevelItemPolygon
    {
        int Id { get; }
        Vec2D[] Points { get; }
        bool IsOutside { get; } //Zeigen die Normalen nach Außen?
    }

    internal interface IEditablePolygon
    {
        float Friction { get; set; }
        float Restiution { get; set; }
        int CollisionCategory { get; set; }
        Vec2D[] Points { get; }
        Vec2D PivotPoint { get; set; }   //Zum Verschieben des Polygons
        bool IsPointInside(Vec2D point);
        void MovePointAtIndex(int index, Vec2D newPosition);
        void RemovePointAtIndex(int index);
        void AddPointAfterIndex(int index, Vec2D newPosition);
    }
}
