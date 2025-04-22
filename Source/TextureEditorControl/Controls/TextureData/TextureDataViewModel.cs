using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Reactive;
using TextureEditorGlobal;
using System.Windows.Forms;
using System.Drawing;

namespace TextureEditorControl.Controls.TextureData
{
    public class TextureDataViewModel : ReactiveObject
    {
        [Reactive] public string TextureFile { get; set; } = "";
        public ReactiveCommand<Unit, Unit> ChangeTextureFileClick { get; private set; }
        //public ReactiveCommand<Unit, Unit> ResetDeltaAngle { get; private set; }
        [Reactive] public bool MakeFirstPixelTransparent { get; set; } = true;
        [Reactive] public float ColorFactor { get; set; } = 1;
        [Reactive] public float DeltaX { get; set; } = 0;
        [Reactive] public float DeltaY { get; set; } = 0;
        [Reactive] public float Width { get; set; } = 1;
        [Reactive] public float Height { get; set; } = 1;
        [Reactive] public float DeltaAngle { get; set; } = 0;
        [Reactive] public float ZValue { get; set; } = 0;
        [Reactive] public bool IsInvisible { get; set; } = false;
        public ReactiveCommand<Unit, Unit> Flip90Click { get; private set; }

        public TextureDataViewModel()
        {
            this.ChangeTextureFileClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "png files (*.png)|*.png|jpeg files (*.jpg)|*.jpg|bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    TextureFile = openFileDialog.FileName;
                }
            });

            this.Flip90Click = ReactiveCommand.Create(() =>
            {
                float temp = this.Width;
                this.Width = this.Height;
                this.Height = temp;
                this.DeltaAngle += 90;
                if (this.DeltaAngle > 180) this.DeltaAngle -= 360;
            });

        }

        public TextureExportData GetExportData()
        {
            int col = Math.Min(255, Math.Max(0, (int)(255 * this.ColorFactor)));

            return new TextureExportData()
            {
                TextureFile = this.TextureFile,
                MakeFirstPixelTransparent = this.MakeFirstPixelTransparent,
                ColorFactor = Color.FromArgb(col, col, col),
                DeltaX = this.DeltaX,
                DeltaY = this.DeltaY,
                Width = this.Width,
                Height = this.Height,
                DeltaAngle = this.DeltaAngle,
                ZValue = this.ZValue,
                IsInvisible = this.IsInvisible,
            };
        }

        public void LoadExportData(TextureExportData data)
        {
            this.TextureFile = data.TextureFile;
            this.MakeFirstPixelTransparent = data.MakeFirstPixelTransparent;
            this.ColorFactor = data.ColorFactor.R / 255f;
            this.DeltaX = data.DeltaX;
            this.DeltaY = data.DeltaY;
            this.Width = data.Width;
            this.Height = data.Height;
            this.DeltaAngle = data.DeltaAngle;
            this.ZValue = data.ZValue;
            this.IsInvisible = data.IsInvisible;
        }
    }
}
