using GraphicPanels;
using GraphicPanelWpf;
using System.Windows.Controls;
using System.Windows.Input;

namespace TextureEditorControl.Controls.Editor
{
    /// <summary>
    /// Interaktionslogik für TextureEditorControl.xaml
    /// </summary>
    public partial class TextureEditorControl : UserControl
    {
        public TextureEditorControl()
        {
            InitializeComponent();
        }

        public TextureEditorControl(TextureEditorViewModel vm, GraphicPanel2D panel)
            : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new GraphicControl(panel);

            this.graphicControlBorder.Cursor = Cursors.None;
        }
    }
}
