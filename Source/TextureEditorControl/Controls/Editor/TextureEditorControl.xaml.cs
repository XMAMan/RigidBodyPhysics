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

        public TextureEditorControl(TextureEditorViewModel vm, DrawingPanel.DrawingPanel panel)
            : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new DrawingPanel.GraphicControl(panel);

            this.graphicControlBorder.Cursor = Cursors.None;
        }
    }
}
