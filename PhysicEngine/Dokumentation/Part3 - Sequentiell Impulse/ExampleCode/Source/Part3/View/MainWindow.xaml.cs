using Part3.ViewModel;
using System.Windows;

namespace Part3.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainWindowViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += (this.DataContext as MainWindowViewModel).HandleKeyDown;
            window.KeyUp += (this.DataContext as MainWindowViewModel).HandleKeyUp;
        }
    }
}
