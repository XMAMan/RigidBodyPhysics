using LevelEditorGlobal;

namespace LevelEditorControl.Controls.PolygonControl
{
    public class PolygonImages
    {
        public ImageData Background = new ImageData();

        public string BackgroundImage
        {
            get
            {
                return this.Background.FileName;
            }
            set
            {
                this.Background.FileName = value;
            }
        }

        public ImageMode BackgroundImageMode
        {
            get
            {
                return this.Background.Mode;
            }
            set
            {
                this.Background.Mode = value;
            }
        }

        public string ForegroundImage;
    }
}
