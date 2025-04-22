using System.Drawing;
using System.IO;

namespace WpfControls.Model
{
    public static class BitmapExtension
    {
        //https://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
        public static System.Drawing.Bitmap ToBitmap(this System.Windows.Media.Imaging.BitmapImage bitmapImage)
        {
            return new System.Drawing.Bitmap(bitmapImage.StreamSource);
        }

        public static System.Windows.Media.Imaging.BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
