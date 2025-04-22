using PhysicSceneEditorControl.Controls.Editor;
using WpfControls.Model;

namespace PhysicSceneEditorControl
{
    public class PhysicSceneEditorFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = (EditorViewModel)CreateEditorViewModel(data);
            var editorControl = new EditorControl(vm, data.Panel);

            if (data.InputData != null)
                vm.LoadFromExportObject(data.InputData);

            return editorControl;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new EditorViewModel(data);
        }
    }
}
