using GraphicMinimal;
using GraphicPanels;
using LevelEditorGlobal;
using System.Drawing;

namespace LevelEditorControl.LevelItems.LawnEdge
{
    //Wird vom Simulator genutzt. Hier kann es nicht mehr editiert werden.
    internal class LawnSegmentBackgroundItem : IBackgroundItem
    {
        private Vector2D center;
        private float angle;
        private float width;
        private float height;
        private string textureFile;
        private float zValue;

        public LawnSegmentBackgroundItem(Vector2D center, float angle, float width, float height, string textureFile, float zValue)
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
                panel.DrawFillRectangle(textureFile, center.Xi, center.Yi, (int)width, (int)height, true, Color.White, angle);
            else
                panel.DrawFillRectangle(Color.Green, center.Xi, center.Yi, (int)width, (int)height, angle);
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
