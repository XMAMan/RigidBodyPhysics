using GraphicPanels;
using LevelEditorGlobal;
using SpriteEditorControl.Controls.Main;
using SpriteEditorControl.Controls.Main.Model;
using SpriteEditorControl.Controls.Sprite.Model;

namespace SpriteEditorControl
{
    //Erzeugt aus ein serialisierten SpriteEditorExportData-Objekt (Wurde über den SpriteEditor erzeugt) ein SpriteData-Objekt
    public static class SpriteDataCreator
    {
        //animationTabIndex = Welchen von den ganzen Animation-Tabs soll zur Bilderzeugung genommen werden?
        public static SpriteData CreateFromSpriteEditorFile(string filePath, int animationTabIndex)
        {
            var editorData = JsonHelper.Helper.CreateFromJson<SpriteEditorExportData>(FileNameReplacer.LoadEditorFile(filePath));

            var panel = new GraphicPanel2D() { Width = 100, Height = 100, Mode = Mode2D.CPU };
            var editorControl = new SpriteEditorFactory().CreateEditorControl(new WpfControls.Model.EditorInputData()
            {
                 DataFolder = ".",
                 InputData = editorData,
                 Panel = panel
            });

            var editor = (SpriteEditorViewModel)editorControl.DataContext;

            return editor.GetSpriteDataFromAnimationTab(animationTabIndex);
        }
    }
}
