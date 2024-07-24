using GraphicPanels;
using Part4.ViewModel.Editor;
using Part4.ViewModel.Simulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Part4.View.Editor
{
    /// <summary>
    /// Interaktionslogik für EditorControl.xaml
    /// </summary>
    public partial class EditorControl : UserControl
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
