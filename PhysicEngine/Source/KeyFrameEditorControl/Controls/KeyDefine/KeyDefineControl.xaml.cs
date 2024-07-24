using GraphicPanels;
using GraphicPanelWpf;
using System.Windows.Controls;

namespace KeyFrameEditorControl.Controls.KeyDefine
{
    /// <summary>
    /// Interaktionslogik für KeyDefineControl.xaml
    /// </summary>
    public partial class KeyDefineControl : UserControl
    {
        public KeyDefineControl()
        {
            InitializeComponent();
        }

        public KeyDefineControl(KeyDefineViewModel vm, GraphicPanel2D panel)
            : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new GraphicControl(panel);
        }
    }
}
