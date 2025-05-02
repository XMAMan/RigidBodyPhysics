using PhysicGlobal;

namespace LevelEditorExports.Editor.LevelItems
{
    public class GroupedItemLevelExportData : ILevelItemExportData
    {
        public int LevelItemId { get; set; }
        public int PrototypId { get; set; }
        public Vec2D Position { get; set; }
        public float SizeFactor { get; set; }
        public float AngleInDegree { get; set; }
        public Vec2D LocalPivot { get; set; }
    }
}
