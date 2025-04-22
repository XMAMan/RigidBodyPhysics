using GraphicMinimal;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    internal class GroupedItemLevelExportData
    {
        public int LevelItemId { get; set; }
        public int PrototypId { get; set; }
        public Vector2D Position { get; set; }
        public float SizeFactor { get; set; }
        public float AngleInDegree { get; set; }
        public Vector2D LocalPivot { get; set; }
    }
}
