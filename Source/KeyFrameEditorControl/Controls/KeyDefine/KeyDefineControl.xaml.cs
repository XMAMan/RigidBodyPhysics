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

        public KeyDefineControl(KeyDefineViewModel vm, DrawingPanel.DrawingPanel panel)
            : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new DrawingPanel.GraphicControl(panel);
        }
    }
}
