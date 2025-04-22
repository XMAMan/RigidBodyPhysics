using ElmaControl.Controls.Main;
using WpfControls.Model;

namespace ElmaControl
{
    public class ElmaControlFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = CreateEditorViewModel(data);
            var control = new Controls.Main.MainControl() { DataContext = vm };

            return control;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new MainViewModel(data);
        }
    }
}
