using GraphicMinimal;

namespace LevelEditorControl.LevelItems.Polygon
{
    internal interface ILevelItemPolygon
    {
        int Id { get; }
        Vector2D[] Points { get; }
        bool IsOutside { get; } //Zeigen die Normalen nach Außen?
    }

    internal interface IEditablePolygon
    {
        float Friction { get; set; }
        float Restiution { get; set; }
        int CollisionCategory { get; set; }
        Vector2D[] Points { get; }
        Vector2D PivotPoint { get; set; }   //Zum Verschieben des Polygons
        bool IsPointInside(Vector2D point);
        void MovePointAtIndex(int index, Vector2D newPosition);
        void RemovePointAtIndex(int index);
        void AddPointAfterIndex(int index, Vector2D newPosition);
    }
}
