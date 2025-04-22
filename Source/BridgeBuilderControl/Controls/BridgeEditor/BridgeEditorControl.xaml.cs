using GraphicPanels;
using GraphicPanelWpf;
using System.Windows.Controls;

namespace BridgeBuilderControl.Controls.BridgeEditor
{
    /// <summary>
    /// Interaktionslogik für BridgeEditorControl.xaml
    /// </summary>
    public partial class BridgeEditorControl : UserControl
    {
        public BridgeEditorControl()
        {
            InitializeComponent();
        }

        internal BridgeEditorControl(BridgeEditorViewModel vm, GraphicPanel2D panel)
           : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new GraphicControl(panel);
        }
    }
}
