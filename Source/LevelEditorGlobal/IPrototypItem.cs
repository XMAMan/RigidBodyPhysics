using GraphicMinimal;
using GraphicPanels;

namespace LevelEditorGlobal
{
    public enum PrototypItemType { PhysicItem, BackgroundItem, GroupedItem }

    //Element vom PrototypControl
    public interface IPrototypItem
    {
        public PrototypItemType ProtoType { get; }
        int Id { get; }
        RectangleF BoundingBox { get; }
        InitialRotatedRectangleValues InitialRecValues { get; } //Mit dem SizeFactor/Angle/Pivot werden LevelItems von diesen Prototyp angelegt
        IPrototypExportData EditorExportData { get; } //Mit diesen Daten kann der Editor der dieses Item erzeugt hat dann neu geladen werden
        Bitmap GetImage(int maxWidth, int maxHeight);
        void Draw(GraphicPanel2D panel); //Zeichnet das Objekt im Bereich von X=0..BoundingBox.Width und Y=0..BoundingBox.Height
        void DrawBorder(GraphicPanel2D panel, Pen borderPen);
        void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor);
    }

    public class InitialRotatedRectangleValues
    {
        public float SizeFactor { get; set; } = 1;//Mit dem SizeFactor werden LevelItems von diesen Prototyp angelegt
        public float AngleInDegree { get; set; } = 0;
        public Vector2D LocalPivot { get; set; } = new Vector2D(0, 0);
    }

    public interface IPrototypExportData
    {
        public PrototypItemType ProtoType { get; }
        public int Id { get; set; }
    }

    public interface IPrototypItemFactory
    {
        IPrototypItem CreatePrototypItem();
    }
}
