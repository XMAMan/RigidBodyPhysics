using System;
using TextureEditorControl.Controls.Editor.Model.Shape;
using TextureEditorControl.Controls.Editor;
using TextureEditorGlobal;
using TexturePhysicImporter;
using WpfControls.Model;
using System.Windows.Controls;
using RigidBodyPhysics.ExportData;

namespace TextureEditorControl
{
    public class TextureEditorFactory : IEditorFactory
    {
        private string physicSceneJson = null;

        public TextureEditorFactory()
        {
        }

        public TextureEditorFactory(string physicSceneJson)
        {
            this.physicSceneJson = physicSceneJson;
        }

        public UserControl CreateEditorControl(EditorInputData data)
        {
            //Wenn der Import-Button nicht gezeigt wird, dann muss zwingend über den TextureEditorViewModel eine PhysicScene reingegeben werden
            if (data.ShowSaveLoadButtons == false && this.physicSceneJson == null)
                throw new ArgumentException("Call the factory with the physicSceneJson-Constructor, if you don't show the Import-Button");

            var vm = (TextureEditorViewModel)CreateEditorViewModel(data);
            var textureEditorControl = new Controls.Editor.TextureEditorControl(vm, data.Panel);

            if (data.InputData != null)
                vm.LoadFromExportObject(data.InputData);

            return textureEditorControl;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new TextureEditorViewModel(data.Panel, data.ShowSaveLoadButtons, this.physicSceneJson);
        }

        public static VisualisizerOutputData CreateDefaultTextureData(PhysicSceneExportData physicSceneData)
        {
            var animationInputData = new PhysicSceneImporter(physicSceneData).Import();
            return new ShapeContainer(animationInputData).GetExportData();
        }
    }
}
