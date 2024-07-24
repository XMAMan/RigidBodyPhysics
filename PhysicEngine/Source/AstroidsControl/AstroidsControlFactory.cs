using AstroidsControl.Controls;
using WpfControls.Model;

namespace AstroidsControl
{
    public class AstroidsControlFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = (MainViewModel)CreateEditorViewModel(data);
            var editorControl = new Controls.MainControl(vm, data.Panel);

            return editorControl;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new MainViewModel(data.Panel, data.SoundGenerator, data.TimerTickRateInMs, data.DataFolder); ;
        }
    }
}
