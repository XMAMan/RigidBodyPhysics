using LevelEditorGlobal;

namespace LevelEditorControl.LevelItems.BackgroundItem
{
    internal class BackgroundPrototypExportData : IPrototypExportData
    {
        public IPrototypItem.Type ProtoType => IPrototypItem.Type.BackgroundItem;
        public int Id { get; set; }
        public string TextureFile { get; set; }
        public float ZValue { get; set; }
        public InitialRotatedRectangleValues InitialRecValues { get; set; }
    }
}
