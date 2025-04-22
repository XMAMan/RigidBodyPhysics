using LevelEditorGlobal;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    internal class GroupedItemProtoExportData : IPrototypExportData
    {
        public IPrototypItem.Type ProtoType => IPrototypItem.Type.GroupedItem;

        public int Id { get; set; }
        public object[] LevelItemsExport { get; set; }
        public InitialRotatedRectangleValues InitialRecValues { get; set; }
    }
}
