using System.Windows;

namespace DrawingPanel
{
    /// <summary>
    /// Interaktionslogik für GraphicControl.xaml
    /// </summary>
    public partial class GraphicControl : System.Windows.Controls.UserControl
    {
        public GraphicControl()
        {
            InitializeComponent();
        }

        public GraphicControl(DrawingPanel panel)
           : this()
        {
            this.panel = panel;

            this.Loaded += this.UserControl_Loaded;
        }

        private DrawingPanel panel;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the interop host control.
            System.Windows.Forms.Integration.WindowsFormsHost host =
                new System.Windows.Forms.Integration.WindowsFormsHost();

            // Assign the MaskedTextBox control as the host control's child.
            host.Child = this.panel.Panel;

            // Add the interop host control to the Grid
            // control's collection of child controls.
            this.graphicGrid.Children.Add(host);
        }
    }
}
