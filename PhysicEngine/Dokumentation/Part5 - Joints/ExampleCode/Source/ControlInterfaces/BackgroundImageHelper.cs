namespace ControlInterfaces
{
    //Da das GraphicPanel bei ClearScreen das Bild immer auf die Panel-Größe skaliert und ich aber nicht will, dass das Bild skaliert
    //wird, fühgt diese Klasse ein weißen Rand noch an das Bild.
    //Vielleicht kann hier später dann auch noch eine Skalierung und Transformation gemacht werden
    public static class BackgroundImageHelper
    {
        public static string ClampImageSize(string sourceImagePath, int width, int height, string outputImagePath)
        {
            var output = ClampImageSize(new Bitmap(sourceImagePath), width, height);
            output.Save(outputImagePath);
            return outputImagePath;
        }

        //Das Rückgabebild ist (width,height) groß. image wird hier ohne Skalierung eingefügt
        public static Bitmap ClampImageSize(Bitmap image, int width, int height)
        {
            Bitmap newImage = new Bitmap(width, height);
            Graphics grx = Graphics.FromImage(newImage);
            grx.Clear(Color.White);
            grx.DrawImage(image, 0, 0, image.Width, image.Height);
            grx.Dispose();
            return newImage;
        }
    }
}
