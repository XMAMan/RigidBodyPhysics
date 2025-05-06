using GraphicPanels;
using LevelEditorExports.Editor.Helper;
using LevelEditorExports.Editor.LevelItems;
using LevelEditorGlobal;
using PhysicGlobal;
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
        bool IsPointInside(Vec2D point);
        ILevelItemExportData GetExportData();
        Vec2D PivotPoint { get; set; } //Hiermit kann das Objekt verschoben werden
        BoundingBox GetBoundingBox();
        Vec2D[] GetCornerPoints(); //4 Eckpunkte vom Physik/Backgrounditem/Lawn oder die Polygonpunkte -> Zum Selektieren per Rechteck
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
