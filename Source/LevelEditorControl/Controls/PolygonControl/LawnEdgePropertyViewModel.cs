using LevelEditorControl.EditorFunctions;
using ReactiveUI;
using System.Reactive;
using System.Windows.Forms;

namespace LevelEditorControl.Controls.PolygonControl
{
    internal class LawnEdgePropertyViewModel : ReactiveObject
    {
        private LawnEdgeDrawer drawer;

        public ReactiveCommand<Unit, Unit> ChangeTextureFileClick { get; private set; }
        public string TextureFile { get => drawer.TextureFile; set => drawer.TextureFile = value; }
        public float ZValue { get => drawer.ZValue; set => drawer.ZValue = value; }
        public float LawnHeight { get => drawer.LawnHeight; set => drawer.LawnHeight = value; }

        public LawnEdgePropertyViewModel(LawnEdgeDrawer drawer)
        {
            this.drawer = drawer;
            this.ChangeTextureFileClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "png files (*.png)|*.png|jpeg files (*.jpg)|*.jpg|bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    TextureFile = openFileDialog.FileName;
                }
            });
        }
    }
}
