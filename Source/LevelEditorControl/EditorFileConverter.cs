using LevelEditorControl.Controls.EditorControl;
using LevelEditorExports.Editor;
using LevelEditorExports.Editor.Helper;
using LevelEditorExports.Simulator;
using WpfControls.Model;

namespace LevelEditorControl
{
    public static class EditorFileConverter
    {
        //Konvertiert ein LevelEditorExportData-Objekt, was zuerst noch aus einer Datei als JSON-String gelesen wird, in ein SimulatorInputData-Objekt
        public static SimulatorInputData Convert(string editorFileName)
        {
            var data = JsonHelper.Helper.CreateFromJson<LevelEditorExportData>(FileNameReplacer.LoadEditorFile(editorFileName));

            var panel = new DrawingPanel.DrawingPanel(100, 100, true);
            var editor = new EditorViewModel(new EditorInputData() { Panel = panel }, new EditorViewModelActions());
            editor.LoadFromExportObject(data);
            return editor.GetSimulatorExport();
        }
    }
}
