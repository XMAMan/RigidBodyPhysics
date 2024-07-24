using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Drawing;
using System.Reactive;
using System.Windows.Media;
using System.Windows.Forms;
using WpfControls.Model;

namespace KeyFrameEditorControl.Dialogs.LoadSprite
{
    public class LoadSpriteViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> LoadSpriteFromFileClick { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadSpriteFromClipboardClick { get; private set; }
        public ReactiveCommand<Unit, Unit> CopyToClipboardClick { get; private set; }
        [Reactive] public int XCount { get; set; } = 10; //Anzahl der Einzelbilder pro Zeile  
        [Reactive] public int YCount { get; set; } = 2; //Anzahl der Einzelbilder pro Spalte
        [Reactive] public ImageSource OriginalImage { get; set; } //Originalbild
        [Reactive] public ImageSource ModifiedImage { get; set; } //Überarbeitetes Bild
        [Reactive] public Bitmap Image { get; set; } = null; //Wenn UseCorrectedImage dann steht hier ModifiedImage ansosnten OriginalImage
        [Reactive] public int SpriteNr { get; set; } = 0; //Dieses Bild wird gerade angezeigt
        [Reactive] public int ImageCount { get; set; } = 9; //Entspricht XCount*YCount

        [Reactive] public int SingleImageWidth { get; set; } //Entspricht new Bitmap(ImageSource).Widht / XCount
        [Reactive] public int SingleImageHeight { get; set; } //Entspricht new Bitmap(ImageSource).Height / YCount

        [Reactive] public float Zoom { get; set; } = 1;
        [Reactive] public float XPosition { get; set; } = 0; //Linke obere Ecke vom SingleBild, was gezeichnet werden soll
        [Reactive] public float YPosition { get; set; } = 0; //Linke obere Ecke vom SingleBild, was gezeichnet werden soll
        [Reactive] public bool UseCorrectedImage { get; set; } = true;
        [Reactive] public bool ShowAnimatedSprite { get; set; } = false;
        [Reactive] public float AnimationSpeed { get; set; } = 0.1f;//Wiedergabegeschwindigkeit von der Spritevorschau
        public float SpriteAnimaitonPosition = 0; //Geht von 0 .. ImageCount

        public LoadSpriteViewModel()
        {
            this.LoadSpriteFromFileClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "png files (*.png)|*.png|jpeg files (*.jpg)|*.jpg|bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.OriginalImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(openFileDialog.FileName));


                    var correctedImage = SpriteBitmapHelper.GetCorrectedSpriteImage(new Bitmap(openFileDialog.FileName), out int xCount, out int yCount, out Bitmap testOutput);
                    this.ModifiedImage = testOutput.ToBitmapImage();

                    if (this.UseCorrectedImage)
                    {
                        this.XCount = xCount;
                        this.YCount = yCount;
                        this.Image = correctedImage;

                    }
                    else
                    {
                        this.Image = new Bitmap(openFileDialog.FileName);
                    }
                }
            });


            this.LoadSpriteFromClipboardClick = ReactiveCommand.Create(() =>
            {
                System.Drawing.Image returnImage = null;
                if (Clipboard.ContainsImage())
                {
                    //https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.clipboard.getimage?view=windowsdesktop-7.0

                    var clipboardImage = Clipboard.GetImage() as Bitmap;
                    if (clipboardImage != null)
                    {
                        var correctedImage = SpriteBitmapHelper.GetCorrectedSpriteImage(clipboardImage, out int xCount, out int yCount, out Bitmap testOutput);
                        this.OriginalImage = clipboardImage.ToBitmapImage();
                        this.ModifiedImage = testOutput.ToBitmapImage();

                        //testOutput.Save(@"..\..\..\..\..\Data\Sprites\Output.bmp");

                        if (this.UseCorrectedImage)
                        {
                            this.XCount = xCount;
                            this.YCount = yCount;
                            this.Image = correctedImage;
                        }
                        else
                        {
                            this.Image = clipboardImage;
                        }
                    }

                }
            });

            this.CopyToClipboardClick = ReactiveCommand.Create(() =>
            {
                if (this.Image != null)
                    Clipboard.SetImage(this.Image);
            });

            this.WhenAnyValue(x => x.XCount, y => y.YCount).Subscribe(group =>
            {
                this.ImageCount = group.Item1 * group.Item2 - 1;
            });

            this.WhenAnyValue(x => x.Image, y => y.XCount, z => z.YCount).Subscribe(group =>
            {
                if (this.Image != null)
                {
                    this.SingleImageWidth = this.Image.Width / this.XCount;
                    this.SingleImageHeight = this.Image.Height / this.YCount;
                }
            });


        }
    }
}
