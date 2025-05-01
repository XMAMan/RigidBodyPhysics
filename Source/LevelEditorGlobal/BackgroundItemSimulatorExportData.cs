using PhysicGlobal;

namespace LevelEditorGlobal
{
    public class BackgroundItemSimulatorExportData
    {
        public Vec2D Center { get; set; }
        public float AngleInDegree { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string TextureFile { get; set; }
        public float ZValue { get; set; }

        public BackgroundItemSimulatorExportData() { }

        public BackgroundItemSimulatorExportData(BackgroundItemSimulatorExportData copy)
        {
            this.Center = new Vec2D(copy.Center);
            this.AngleInDegree = copy.AngleInDegree;
            this.Width = copy.Width;
            this.Height = copy.Height;
            TextureFile = copy.TextureFile;
            ZValue = copy.ZValue;
        }
    }
}
