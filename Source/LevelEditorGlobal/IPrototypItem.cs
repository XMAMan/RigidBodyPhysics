using GraphicPanels;
using LevelEditorExports.Editor.Prototyps;

namespace LevelEditorGlobal
{
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

    public interface IPrototypItemFactory
    {
        IPrototypItem CreatePrototypItem();
    }
}
