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

        public SpriteControl(DrawingPanel.DrawingPanel panel)
            : this()
        {
            this.graphicControlBorder.Child = new DrawingPanel.GraphicControl(panel);
        }
    }
}
