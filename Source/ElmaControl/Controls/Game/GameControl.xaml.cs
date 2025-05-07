using System.Windows.Controls;

namespace ElmaControl.Controls.Game
{
    /// <summary>
    /// Interaktionslogik für GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        public GameControl()
        {
            InitializeComponent();
        }

        internal GameControl(GameViewModel vm, DrawingPanel.DrawingPanel panel)
           : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new DrawingPanel.GraphicControl(panel);
        }
    }
}
