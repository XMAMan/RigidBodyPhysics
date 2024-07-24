using EditorControl.ViewModel;
using GraphicPanels;
using GraphicPanelWpf;

namespace EditorControl.View
{
    /// <summary>
    /// Interaktionslogik für EditorControl.xaml
    /// </summary>
    public partial class EditorControl : System.Windows.Controls.UserControl
    {
        public EditorControl()
        {
            InitializeComponent();
        }

        public EditorControl(EditorViewModel vm, GraphicPanel2D panel)
            : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new GraphicControl(panel);
        }
    }
}
