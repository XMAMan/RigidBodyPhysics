using SpriteEditorControl.Controls.Main;
using WpfControls.Model;

namespace SpriteEditorControl
{
    public class SpriteEditorFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = (SpriteEditorViewModel)CreateEditorViewModel(data);
            var editorControl = new View.SpriteEditorControl() { DataContext = vm };

            if (data.InputData != null)
            {
                vm.LoadFromExportObject(data.InputData);
            }
                

            return editorControl;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new SpriteEditorViewModel(data);
        }
    }
}
