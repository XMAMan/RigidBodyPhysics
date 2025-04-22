using System;
using System.Windows.Controls;
using System.Windows;

namespace WpfControls.View.CollisionMatrix
{
    //Diese Klasse ist für dier Erstellung von ein bool[,]-Objekt für das gilt: RowCount=ColumCount
    //Optionen: IsSymmetricMatrix = true -> Stellt sicher, dass Matrix[x,y] den gleichen Wert wie Matrix[y,x] hat
    public class BoolMatrixGrid : System.Windows.Controls.Grid
    {
        #region BoolMatrix-Property
        public static readonly DependencyProperty BoolMatrixProperty =
            DependencyProperty.Register("BoolMatrix", typeof(bool[,]), typeof(BoolMatrixGrid),
         new FrameworkPropertyMetadata((bool[,])new bool[1, 1], FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeBoolMatrix)));
        private static void ChangeBoolMatrix(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as BoolMatrixGrid).BuildGrid();
            }
        }
        public bool[,] BoolMatrix
        {
            get => (bool[,])GetValue(BoolMatrixProperty);
            set => SetValue(BoolMatrixProperty, value);
        }
        #endregion

        #region IsSymmetricMatrix
        public static readonly DependencyProperty IsSymmetricMatrixProperty =
            DependencyProperty.Register("IsSymmetricMatrix", typeof(bool), typeof(BoolMatrixGrid),
         new FrameworkPropertyMetadata((bool)true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeIsSymmetricMatrix)));
        private static void ChangeIsSymmetricMatrix(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as BoolMatrixGrid).DoSymmetricCheck();
            }
        }
        public bool IsSymmetricMatrix
        {
            get => (bool)GetValue(IsSymmetricMatrixProperty);
            set => SetValue(IsSymmetricMatrixProperty, value);
        }
        #endregion

        struct XY
        {
            public int X; public int Y;
            public XY(int x, int y)
            {
                this.X = x; this.Y = y;
            }
        }

        private void BuildGrid()
        {
            var matrix = this.BoolMatrix;

            if (matrix.GetLength(0) != matrix.GetLength(1)) throw new ArgumentException("matrix must be quadratic");
            int size = matrix.GetLength(0);

            this.Children.Clear();
            this.RowDefinitions.Clear();
            this.ColumnDefinitions.Clear();

            for (int i = 0; i <= size; i++)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });
                this.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });
            }

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var box = new System.Windows.Controls.CheckBox();
                    box.Tag = new XY(x, y);
                    box.IsChecked = matrix[x, y];
                    box.SetValue(Grid.ColumnProperty, x + 1);
                    box.SetValue(Grid.RowProperty, y + 1);
                    box.Checked += Box_Checked;
                    box.Unchecked += Box_Unchecked;
                    this.Children.Add(box);
                }

            for (int i = 0; i <= size; i++)
            {
                if (i < size)
                {
                    var label1 = new System.Windows.Controls.Label() { Content = i.ToString(), Margin = new Thickness(0, -7, 0, 0) };
                    label1.SetValue(Grid.ColumnProperty, i + 1);
                    label1.SetValue(Grid.RowProperty, 0);
                    this.Children.Add(label1);

                    var label2 = new System.Windows.Controls.Label() { Content = i.ToString(), Margin = new Thickness(0, -7, 0, 0) };
                    label2.SetValue(Grid.ColumnProperty, 0);
                    label2.SetValue(Grid.RowProperty, i + 1);
                    this.Children.Add(label2);
                }
            }

            if (this.IsSymmetricMatrix) DoSymmetricCheck();
        }


        private bool avoidRecursionFlag = true;

        private void Box_Checked(object sender, RoutedEventArgs e)
        {
            var xy = (XY)(sender as System.Windows.Controls.CheckBox).Tag;
            this.BoolMatrix[xy.X, xy.Y] = true;

            if (this.IsSymmetricMatrix && this.avoidRecursionFlag)
            {
                this.avoidRecursionFlag = false;
                GetBox(xy.Y, xy.X).IsChecked = true;
                this.avoidRecursionFlag = true;
            }

        }
        private void Box_Unchecked(object sender, RoutedEventArgs e)
        {
            var xy = (XY)(sender as System.Windows.Controls.CheckBox).Tag;
            this.BoolMatrix[xy.X, xy.Y] = false;

            if (this.IsSymmetricMatrix && this.avoidRecursionFlag)
            {
                this.avoidRecursionFlag = false;
                GetBox(xy.Y, xy.X).IsChecked = false;
                this.avoidRecursionFlag = true;
            }
        }

        private System.Windows.Controls.CheckBox GetBox(int x, int y)
        {
            return (this.Children[y * this.BoolMatrix.GetLength(0) + x] as System.Windows.Controls.CheckBox);
        }

        private void DoSymmetricCheck()
        {
            int size = this.BoolMatrix.GetLength(0);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    if (this.BoolMatrix[x, y] != this.BoolMatrix[y, x])
                    {
                        GetBox(y, x).IsChecked = this.BoolMatrix[x, y];
                    }
                }
        }
    }
}
