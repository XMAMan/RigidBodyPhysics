using GraphicPanels;
using GraphicPanelWpf;
using KeyFramePhysicImporter.ViewModel;

namespace KeyFramePhysicImporter.View
{
    /// <summary>
    /// Interaktionslogik für ImporterControl.xaml
    /// </summary>
    public partial class ImporterControl : System.Windows.Controls.UserControl
    {
        public ImporterControl()
        {
            InitializeComponent();
        }

        public ImporterControl(ImporterControlViewModel vm, GraphicPanel2D panel)
            : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new GraphicControl(panel);
        }
    }
}
