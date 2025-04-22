namespace SpriteEditorControl.Controls.Sprite
{
    public class SpriteExportData
    {
        public enum PivotOriantation { None, Bottom }

        public int SpriteCount { get; set; }
        public SpriteExportData.PivotOriantation Oriantation { get; set; }
        public int TimeStepsPerFrame { get; set; }
        public int IterationCount { get; set; }
        public int PivotX { get; set; }
        public int PivotY { get; set;}
        public float Zoom { get; set; }
        public float RotateZ { get; set; }
        public float RotateY { get; set; }
    }
}
