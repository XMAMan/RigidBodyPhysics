using System.Drawing;
using System.IO;

namespace SpriteEditorControl.Controls.Sprite.Model
{
    internal static class BitmapExtensions
    {
        public static void SaveWithCorrectFormat(this Bitmap bitmap, string fileName)
        {
            string extension = new FileInfo(fileName).Extension.ToLower();
            System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Bmp;
            switch (extension)
            {
                case ".png":
                    format = System.Drawing.Imaging.ImageFormat.Png; break;

                case ".jpeg":
                case ".jpg":
                    format = System.Drawing.Imaging.ImageFormat.Jpeg; break;

                case ".bmp":
                    format = System.Drawing.Imaging.ImageFormat.Bmp; break;
            }

            bitmap.Save(fileName, format);
        }
    }
}
