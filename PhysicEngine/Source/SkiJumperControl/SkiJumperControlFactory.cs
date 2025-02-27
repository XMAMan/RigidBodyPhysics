using DemoApplicationHelper;
using SkiJumperControl.Controls;
using WpfControls.Model;

namespace SkiJumperControl
{
    public class SkiJumperControlFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = (MainViewModel)CreateEditorViewModel(data);
            var editorControl = new Controls.MainControl(vm, data.Panel);

            return editorControl;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new MainViewModel(data.Panel, (data as EditorInputDataWithSound).SoundGenerator, data.TimerTickRateInMs, data.DataFolder);
        }
    }
}
