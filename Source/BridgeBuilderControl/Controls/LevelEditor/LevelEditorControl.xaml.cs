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

        internal LevelEditorControl(LevelEditorViewModel vm, DrawingPanel.DrawingPanel panel)
           : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new DrawingPanel.GraphicControl(panel);
        }
    }
}
