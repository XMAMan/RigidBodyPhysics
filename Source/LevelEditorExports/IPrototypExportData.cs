namespace LevelEditorExports
{
    public enum PrototypItemType { PhysicItem, BackgroundItem, GroupedItem }

    public interface IPrototypExportData
    {
        public PrototypItemType ProtoType { get; }
        public int Id { get; set; }
    }
}
