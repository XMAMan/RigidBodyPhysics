using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;

namespace EditorControl.ViewModel
{
    internal class BackgroundImageViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> LoadImageFromFileClick { get; private set; }
        [Reactive] public float Zoom { get; set; } = 1;
        [Reactive] public float XPosition { get; set; } = 0; //Linke obere Ecke vom Bild, was gezeichnet werden soll
        [Reactive] public float YPosition { get; set; } = 0; //Linke obere Ecke vom Bild, was gezeichnet werden soll
        [Reactive] public Bitmap Image { get; set; } = null; //Eingeladenes Bild

        public BackgroundImageViewModel()
        {
            this.LoadImageFromFileClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "png files (*.png)|*.png|jpeg files (*.jpg)|*.jpg|bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.Image = new Bitmap(openFileDialog.FileName);
                }
            });
        }
    }
}
