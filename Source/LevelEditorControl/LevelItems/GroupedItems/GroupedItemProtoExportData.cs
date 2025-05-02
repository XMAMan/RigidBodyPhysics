using LevelEditorExports.Editor.Prototyps;
using LevelEditorGlobal;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    internal class GroupedItemProtoExportData : IPrototypExportData
    {
        public PrototypItemType ProtoType => PrototypItemType.GroupedItem;

        public int Id { get; set; }
        public object[] LevelItemsExport { get; set; }
        public InitialRotatedRectangleValues InitialRecValues { get; set; }
    }
}
