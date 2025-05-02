using LevelEditorExports.Editor.LevelItems;

namespace LevelEditorExports.Editor.Prototyps
{
    public class GroupedItemProtoExportData : IPrototypExportData
    {
        public PrototypItemType ProtoType => PrototypItemType.GroupedItem;

        public int Id { get; set; }
        public ILevelItemExportData[] LevelItemsExport { get; set; }
        public InitialRotatedRectangleValues InitialRecValues { get; set; }
    }
}
