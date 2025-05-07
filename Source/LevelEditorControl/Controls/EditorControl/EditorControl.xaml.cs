namespace LevelEditorControl.Controls.EditorControl
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

        internal EditorControl(EditorViewModel vm, DrawingPanel.DrawingPanel panel)
            : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new DrawingPanel.GraphicControl(panel);
        }
    }
}
