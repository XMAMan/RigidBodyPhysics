using LevelEditorControl.Controls.LevelEditorControl1;
using WpfControls.Model;

namespace LevelEditorControl
{
    public class LevelEditorFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = (LevelEditorViewModel)CreateEditorViewModel(data);
            var editorControl = new Controls.LevelEditorControl1.LevelEditorControl() { DataContext = vm };

            if (data.InputData != null)
            {
                vm.LoadFromExportObject(data.InputData);
            }
                

            return editorControl;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new LevelEditorViewModel(data);
        }
    }
}
