namespace LevelEditorGlobal
{
    public enum ImageMode
    {
        StretchWithoutAspectRatio,   //Hintergrundbild wird verzehrt im Hintergrund gemalt
        StretchWithAspectRatio,      //Hintergrundbild wird an der Stelle (0,0) so groß wie möglich gemalt
        NoStretch                    //Hintergrundbild wird an der Stelle (0,0) bis (ImageWidth,ImageHeight) gemalt
    }

    //Speichert die Daten für das Background-Bild im LevelEditor
    public class ImageData
    {
        private string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
                if (string.IsNullOrEmpty(fileName) == false)
                {
                    this.Size = new Bitmap(fileName).Size;
                }
            }
        }
        public Size Size { get; private set; }
        public ImageMode Mode = ImageMode.StretchWithoutAspectRatio;

        public ImageData() { }

        public ImageData(ImageData copy)
        {
            this.fileName = copy.FileName;
            this.Size = copy.Size;
            this.Mode = copy.Mode;
        }
    }
}
