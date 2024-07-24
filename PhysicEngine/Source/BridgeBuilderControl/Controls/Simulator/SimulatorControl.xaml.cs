using GraphicPanels;
using GraphicPanelWpf;
using System.Windows.Controls;

namespace BridgeBuilderControl.Controls.Simulator
{
    /// <summary>
    /// Interaktionslogik für SimulatorControl.xaml
    /// </summary>
    public partial class SimulatorControl : UserControl
    {
        public SimulatorControl()
        {
            InitializeComponent();
        }

        internal SimulatorControl(SimulatorViewModel vm, GraphicPanel2D panel)
           : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new GraphicControl(panel);
        }
    }
}
