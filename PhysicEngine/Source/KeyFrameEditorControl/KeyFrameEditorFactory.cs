using KeyFrameEditorControl.Controls.KeyFrameEditor;
using System;
using WpfControls.Model;

namespace KeyFrameEditorControl
{
    public class KeyFrameEditorFactory : IEditorFactory
    {
        private string physicSceneJson = null;
        private bool showStartTimeTextbox = true;

        public KeyFrameEditorFactory()
        {
        }

        public KeyFrameEditorFactory(string physicSceneJson, bool showStartTimeTextbox)
        {
            this.physicSceneJson = physicSceneJson;
            this.showStartTimeTextbox = showStartTimeTextbox;
        }

        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            //Wenn der Import-Button nicht gezeigt wird, dann muss zwingend über den TextureEditorViewModel eine PhysicScene reingegeben werden
            if (data.ShowSaveLoadButtons == false && this.physicSceneJson == null)
                throw new ArgumentException("Call the factory with the physicSceneJson-Constructor, if you don't show the Import-Button");


            var vm = (KeyFrameEditorViewModel)CreateEditorViewModel(data);
            var keyFrameAnimatorControl = new Controls.KeyFrameEditor.KeyFrameEditorControl() { DataContext = vm };

            if (data.InputData != null)
                vm.LoadFromExportObject(data.InputData);

            return keyFrameAnimatorControl;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new KeyFrameEditorViewModel(data.Panel, data.TimerTickRateInMs, data.ShowSaveLoadButtons, this.showStartTimeTextbox, this.physicSceneJson);
        }
    }
}
