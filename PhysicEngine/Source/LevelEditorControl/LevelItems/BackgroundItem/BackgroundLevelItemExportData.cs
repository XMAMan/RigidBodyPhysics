using GraphicMinimal;

namespace LevelEditorControl.LevelItems.BackgroundItem
{
    internal class BackgroundLevelItemExportData
    {
        public int LevelItemId { get; set; }
        public int PrototypId { get; set; }
        public Vector2D Position { get; set; }
        public float SizeFactor { get; set; }
        public float AngleInDegree { get; set; }
        public Vector2D LocalPivot { get; set; }
    }
}
