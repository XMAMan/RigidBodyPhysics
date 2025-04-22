using GraphicPanels;
using GraphicPanelWpf;
using System.Windows.Controls;

namespace BridgeBuilderControl.Controls.LevelEditor
{
    /// <summary>
    /// Interaktionslogik für LevelEditorControl.xaml
    /// </summary>
    public partial class LevelEditorControl : UserControl
    {
        public LevelEditorControl()
        {
            InitializeComponent();
        }

        internal LevelEditorControl(LevelEditorViewModel vm, GraphicPanel2D panel)
           : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new GraphicControl(panel);
        }
    }
}
