using GraphicPanels;
using GraphicPanelWpf;
using System.Windows.Controls;

namespace SpriteEditorControl.Controls.Sprite
{
    /// <summary>
    /// Interaktionslogik für SpriteExportControl.xaml
    /// </summary>
    public partial class SpriteControl : UserControl
    {
        public SpriteControl()
        {
            InitializeComponent();
        }

        public SpriteControl(GraphicPanel2D panel)
            : this()
        {
            this.graphicControlBorder.Child = new GraphicControl(panel);
        }
    }
}
