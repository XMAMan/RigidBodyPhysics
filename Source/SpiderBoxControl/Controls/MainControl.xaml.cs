using System.Windows.Controls;

namespace SpiderBoxControl.Controls
{
    /// <summary>
    /// Interaktionslogik für MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        public MainControl()
        {
            InitializeComponent();
        }

        internal MainControl(MainViewModel vm, DrawingPanel.DrawingPanel panel)
           : this()
        {
            this.DataContext = vm;

            this.graphicControlBorder.Child = new DrawingPanel.GraphicControl(panel);
        }
    }
}
