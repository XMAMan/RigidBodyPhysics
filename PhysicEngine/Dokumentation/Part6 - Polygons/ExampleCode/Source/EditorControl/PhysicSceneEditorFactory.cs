using EditorControl.ViewModel;
using WpfControls.Model;
using System.Reactive.Linq;

namespace EditorControl
{
    public class PhysicSceneEditorFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = new EditorViewModel(data.Panel, data.ShowSaveLoadButtons);
            vm.SwitchClick.Subscribe(value => { data.IsFinished(); });
            var editorControl = new EditorControl.View.EditorControl(vm, data.Panel);

            if (data.InputData != null)
                vm.LoadFromExportObject(data.InputData);

            return editorControl;
        }
    }
}
