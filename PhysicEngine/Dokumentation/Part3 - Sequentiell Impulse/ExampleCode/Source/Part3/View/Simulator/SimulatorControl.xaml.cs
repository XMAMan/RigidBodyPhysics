using GraphicPanels;
using Part3.ViewModel.Simulator;

namespace Part3.View.Simulator
{
    /// <summary>
    /// Interaktionslogik für SimulatorControl.xaml
    /// </summary>
    public partial class SimulatorControl : System.Windows.Controls.UserControl
    {
        public SimulatorControl()
        {
            InitializeComponent();
        }

        public SimulatorControl(SimulatorViewModel vm, GraphicPanel2D panel)
            :this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new GraphicControl(panel);
        }
    }
}
