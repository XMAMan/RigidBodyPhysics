using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace WpfControls.Controls.CollisionMatrix
{
    //Hiermit kann eine Farbe ausgewählt werden. Über Colors[SelectedIndex] sehe ich welche Farbe ausgewählt wurde
    public class ColorSelector : Grid
    {
        #region ColorCount
        public static readonly DependencyProperty ColorCountProperty =
            DependencyProperty.Register("ColorCount", typeof(byte), typeof(ColorSelector),
         new FrameworkPropertyMetadata((byte)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeColorCount)));
        private static void ChangeColorCount(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as ColorSelector).BuildGrid();
            }
        }
        public byte ColorCount
        {
            get => (byte)GetValue(ColorCountProperty);
            set => SetValue(ColorCountProperty, value);
        }
        #endregion

        #region SelectedIndex
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ColorSelector),
         new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeSelectedIndex)));
        private static void ChangeSelectedIndex(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as ColorSelector).ChangeBorderColor((int)e.NewValue, Color.Black);
            }
        }
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }
        #endregion

        #region Colors
        public static readonly DependencyProperty ColorsProperty =
            DependencyProperty.Register("Colors", typeof(Color[]), typeof(ColorSelector),
         new FrameworkPropertyMetadata((Color[])new Color[] { Color.Red, Color.Green }, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeColors)));
        private static void ChangeColors(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as ColorSelector).BuildGrid();
            }
        }
        public Color[] Colors
        {
            get => (Color[])GetValue(ColorsProperty);
            set => SetValue(ColorsProperty, value);
        }
        #endregion

        #region Orientation
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(System.Windows.Controls.Orientation), typeof(ColorSelector),
         new FrameworkPropertyMetadata(System.Windows.Controls.Orientation.Horizontal, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeOrientation)));
        private static void ChangeOrientation(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as ColorSelector).BuildGrid();
            }
        }
        public System.Windows.Controls.Orientation Orientation
        {
            get => (System.Windows.Controls.Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        #endregion

        private void BuildGrid()
        {
            this.Children.Clear();
            this.RowDefinitions.Clear();
            this.ColumnDefinitions.Clear();

            if (this.Orientation == System.Windows.Controls.Orientation.Horizontal)
                this.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });
            else
                this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });

            for (int i = 0; i < this.ColorCount; i++)
            {
                if (this.Orientation == System.Windows.Controls.Orientation.Horizontal)
                    this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20) });
                else
                    this.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });



                var border = new Border();
                border.Width = 20;
                border.Height = 20;
                border.BorderThickness = new Thickness(3);
                if (this.Orientation == System.Windows.Controls.Orientation.Horizontal)
                {
                    border.SetValue(Grid.ColumnProperty, i);
                    border.SetValue(Grid.RowProperty, 0);
                }
                else
                {
                    border.SetValue(Grid.ColumnProperty, 0);
                    border.SetValue(Grid.RowProperty, i);
                }

                this.Children.Add(border);

                var button = new System.Windows.Controls.Button();
                button.Tag = i;
                button.Click += Button_Click;
                border.Child = button;
                button.Background = ColorToBrush(this.Colors[i % this.Colors.Length]);
            }

            this.SelectedIndex = 0;
            if (this.Children.Count > 0)
                ChangeBorderColor(this.SelectedIndex, Color.Black);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int index = (int)(sender as System.Windows.Controls.Button).Tag;
            this.SelectedIndex = index;
        }

        private void ChangeBorderColor(int index, Color color)
        {
            SetAllBorderColors(Color.White);
            SetBorderColor(index, Color.Black);
        }

        private void SetAllBorderColors(Color color)
        {
            for (int i = 0; i < this.Children.Count; i++)
            {
                SetBorderColor(i, color);
            }
        }

        private void SetBorderColor(int index, Color color)
        {
            (this.Children[index] as Border).BorderBrush = ColorToBrush(color);
        }

        private static System.Windows.Media.SolidColorBrush ColorToBrush(Color color)
        {
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(color.R, color.G, color.B));
        }
    }
}
