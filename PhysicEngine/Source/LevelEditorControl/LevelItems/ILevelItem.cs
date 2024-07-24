using GraphicMinimal;
using GraphicPanels;
using LevelEditorGlobal;
using System.Drawing;

namespace LevelEditorControl.LevelItems
{
    //PhysicItem, BackgroundItem, Polygon, LawnEdge
    internal interface ILevelItem
    {
        int Id { get; }
        bool IsSelected { get; set; }
        float GetArea();
        void Draw(GraphicPanel2D panel);
        void DrawBorder(GraphicPanel2D panel, Pen borderPen);
        void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor);
        bool IsPointInside(Vector2D point);
        object GetExportData();
        Vector2D PivotPoint { get; set; } //Hiermit kann das Objekt verschoben werden
        RectangleF GetBoundingBox();
        Vector2D[] GetCornerPoints(); //4 Eckpunkte vom Physik/Backgrounditem/Lawn oder die Polygonpunkte -> Zum Selektieren per Rechteck
    }

    internal interface IRotateableLevelItem : ILevelItem
    {
        RotatedRectangle RotatedRectangle { get; }
    }

    //PhysicItem, BackgroundItem
    internal interface IPrototypLevelItem : ILevelItem
    {
        IPrototypItem AssociatedPrototyp { get; }
        void UpdateAfterPrototypWasChanged(IPrototypItem oldItem, IPrototypItem newItem);
        IPrototypLevelItem CreateCopy(int newId); //Wird von der GroupItemsFunction genutzt
    }
}
