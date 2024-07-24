using System.Drawing;
using System.IO;

namespace TextureEditorControl.Controls.Editor.Model
{
    static class CursoreHelper
    {
        //https://stackoverflow.com/questions/46805/custom-cursor-in-wpf
        public static System.Windows.Input.Cursor BitmapToCursor(Bitmap bitmap)
        {
            //Von Resource würde so gehen:
            //this.DrawingPanelCursor = new System.Windows.Input.Cursor(new System.IO.MemoryStream(TextureEditor.Properties.Resources.CursoreRotate));

            // Save to .ico format
            var stream = new MemoryStream();
            System.Drawing.Icon.FromHandle(bitmap.GetHicon()).Save(stream);

            // Convert saved file into .cur format
            stream.Seek(2, SeekOrigin.Begin);
            stream.WriteByte(2);
            stream.Seek(10, SeekOrigin.Begin);
            stream.WriteByte((byte)(int)(bitmap.Width / 2));
            stream.WriteByte((byte)(int)(bitmap.Height / 2));
            stream.Seek(0, SeekOrigin.Begin);

            return new System.Windows.Input.Cursor(stream);
        }
    }
}
