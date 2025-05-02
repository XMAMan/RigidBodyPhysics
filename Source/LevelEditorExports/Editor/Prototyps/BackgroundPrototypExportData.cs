namespace LevelEditorExports.Editor.Prototyps
{
    public class BackgroundPrototypExportData : IPrototypExportData
    {
        public PrototypItemType ProtoType => PrototypItemType.BackgroundItem;
        public int Id { get; set; }
        public string TextureFile { get; set; }
        public float ZValue { get; set; }
        public InitialRotatedRectangleValues InitialRecValues { get; set; }
    }
}
