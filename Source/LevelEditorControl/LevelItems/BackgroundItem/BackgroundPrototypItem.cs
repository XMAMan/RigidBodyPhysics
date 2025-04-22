using GraphicPanels;
using LevelEditorGlobal;
using System.Drawing;
using WpfControls.Controls.CameraSetting;

namespace LevelEditorControl.LevelItems.BackgroundItem
{
    //Dieses Objekt wird in der Prototypbox dann angezeigt
    internal class BackgroundPrototypItem : IPrototypItem
    {
        private string textureFile;
        private Bitmap image;
        public BackgroundPrototypItem(string textureFile, int id, InitialRotatedRectangleValues initialRecValues)
        {
            this.Id = id;
            this.textureFile = textureFile;
            this.image = new Bitmap(textureFile);
            this.BoundingBox = new RectangleF(0, 0, image.Width, image.Height);
            this.InitialRecValues = initialRecValues;
        }

        public IPrototypItem.Type ProtoType { get => IPrototypItem.Type.BackgroundItem; }
        public int Id { get; }
        public RectangleF BoundingBox { get; }
        public InitialRotatedRectangleValues InitialRecValues { get; }
        public IPrototypExportData EditorExportData { get => CreateExportData(); } //Mit diesen Daten kann der Editor der dieses Item erzeugt hat dann neu geladen werden

        private BackgroundPrototypExportData CreateExportData()
        {
            return new BackgroundPrototypExportData()
            {
                Id = this.Id,
                TextureFile = this.textureFile,
                ZValue = this.ZValue,
                InitialRecValues = this.InitialRecValues,
            };
        }

        public static BackgroundPrototypItem CreateFromExportData(BackgroundPrototypExportData data)
        {
            if (data.InitialRecValues == null) data.InitialRecValues = new InitialRotatedRectangleValues();
            return new BackgroundPrototypItem(data.TextureFile, data.Id, data.InitialRecValues) { ZValue = data.ZValue };
        }

        public Bitmap GetImage(int maxWidth, int maxHeight)
        {
            var panel = new GraphicPanel2D() { Width = maxWidth, Height = maxHeight, Mode = Mode2D.CPU };
            var camera = new Camera2D(maxWidth, maxHeight, this.BoundingBox);

            panel.ClearScreen(Color.White);
            panel.DrawFillRectangle(this.textureFile, 0, 0, (int)camera.LengthToScreen(image.Width), (int)camera.LengthToScreen(image.Height), true, Color.White);
            panel.FlipBuffer();

            return panel.GetScreenShoot();
        }
        public void Draw(GraphicPanel2D panel)
        {
            panel.ZValue2D = this.ZValue;
            panel.DrawFillRectangle(this.textureFile, 0, 0, image.Width, image.Height, true, Color.White);
        }
        public void DrawBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.DrawRectangle(borderPen, 0, 0, image.Width, image.Height);
        }
        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            panel.ZValue2D = this.ZValue;
            panel.DrawFillRectangle(frontColor, 0, 0, image.Width, image.Height);
        }
        public float ZValue { get; set; }
    }
}
