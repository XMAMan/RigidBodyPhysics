namespace LevelEditorExports.Editor.LevelItems
{
    public class LawnEdgeExportData : ILevelItemExportData
    {
        public int LevelItemId { get; set; } //eigene Id
        public int PolygonLevelItemId { get; set; } //Id von den Polygon wo dieses Objekt dranhängt
        public string TextureFile { get; set; }
        public float ZValue { get; set; }
        public float LawnHeight { get; set; }
        public int Index1 { get; set; }
        public float FPos1 { get; set; }
        public int Index2 { get; set; }
        public float FPos2 { get; set; }
    }
}
