using GraphicPanels;
using LevelEditorGlobal;
using PhysicGlobal;
using System.Drawing;

namespace LevelEditorControl.LevelItems.LawnEdge
{
    //Wird vom Simulator genutzt. Hier kann es nicht mehr editiert werden.
    internal class LawnSegmentBackgroundItem : IBackgroundItem
    {
        private Vec2D center;
        private float angle;
        private float width;
        private float height;
        private string textureFile;
        private float zValue;

        public LawnSegmentBackgroundItem(Vec2D center, float angle, float width, float height, string textureFile, float zValue)
        {
            this.center = center;
            this.angle = angle;
            this.width = width;
            this.height = height;
            this.textureFile = textureFile;
            this.zValue = zValue;
        }
        public void Draw(GraphicPanel2D panel)
        {
            panel.ZValue2D = zValue;

            if (string.IsNullOrEmpty(textureFile) == false)
                panel.DrawFillRectangle(textureFile, (int)center.X, (int)center.Y, (int)width, (int)height, true, Color.White, angle);
            else
                panel.DrawFillRectangle(Color.Green, (int)center.X, (int)center.Y, (int)width, (int)height, angle);
        }

        public BackgroundItemSimulatorExportData GetSimulatorExportData()
        {
            return new BackgroundItemSimulatorExportData()
            {
                Center = center,
                AngleInDegree = angle,
                Width = width,
                Height = height,
                TextureFile = textureFile,
                ZValue = zValue
            };
        }
    }
}
