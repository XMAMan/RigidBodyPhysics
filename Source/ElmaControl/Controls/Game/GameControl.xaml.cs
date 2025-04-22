using GraphicPanels;
using GraphicPanelWpf;
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

        internal GameControl(GameViewModel vm, GraphicPanel2D panel)
           : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new GraphicControl(panel);
        }
    }
}
