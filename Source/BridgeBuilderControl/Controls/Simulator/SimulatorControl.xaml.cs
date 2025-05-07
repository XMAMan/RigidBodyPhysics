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

        internal SimulatorControl(SimulatorViewModel vm, DrawingPanel.DrawingPanel panel)
           : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new DrawingPanel.GraphicControl(panel);
        }
    }
}
