using WpfControls.Model;

namespace PhysicSceneTestbedControl
{
    public class PhysicSceneTestbedFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = (PhysicSceneTestbedViewModel)CreateEditorViewModel(data);
            var editorControl = new PhysicSceneTestbedControl() { DataContext = vm };

            if (data.InputData != null)
                vm.LoadFromExportObject(data.InputData);

            return editorControl;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new PhysicSceneTestbedViewModel(data.Panel, data.TimerTickRateInMs);
        }
    }
}
