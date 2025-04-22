using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Drawing;

namespace WpfControls.ViewModel
{
    public class CollisionMatrixViewModel : ReactiveObject
    {
        [Reactive] public bool[,] CollideMatrix { get; set; }
        [Reactive] public int IndexCount { get; set; } //Anzahl der farbigen Kächsten über der Matrix
        [Reactive] public Color[] Colors { get; set; } = new Color[] { Color.Yellow, Color.Green, Color.Blue, Color.Red, Color.Orange, Color.Orchid };
        [Reactive] public int SelectedIndex { get; set; } = 0;

        public CollisionMatrixViewModel(bool[,] matrix)
        {
            this.CollideMatrix = matrix;
            this.IndexCount = matrix.GetLength(0);
        }

        public CollisionMatrixViewModel(int matrixSize)
        {
            this.CollideMatrix = new bool[matrixSize, matrixSize];
            this.IndexCount = matrixSize;
            this.CollideMatrix[0, 0] = true;
        }
    }
}
