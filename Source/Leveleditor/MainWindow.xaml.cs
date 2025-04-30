using System.Windows;

namespace Leveleditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new ViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);

            //Wenn ich im Leveleditor was simuliere und die Pfeiltasten oder Space drücke, dann verliert der 
            //Start-Simulation-Button sein Keyboard-Focus und ich aktiviere dann die Gravity-Textbox. Diese 
            //verhindert dann, dass Tastendrücke zum MainWindowViewModel hier weitergeleitet werden
            //Deswegen nutze ich hier die Preview-Events da man so die Textbox austrickst
            window.PreviewKeyDown += (this.DataContext as ViewModel).HandleKeyDown;
            window.PreviewKeyUp += (this.DataContext as ViewModel).HandleKeyUp;
        }
    }
}