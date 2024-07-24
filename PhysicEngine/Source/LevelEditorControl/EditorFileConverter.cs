using GraphicPanels;
using LevelEditorControl.Controls.EditorControl;
using LevelEditorControl.Controls.PrototypControl;
using LevelEditorGlobal;
using PhysicItemEditorControl.Model;
using System.Linq;
using WpfControls.Model;

namespace LevelEditorControl
{

    public static class EditorFileConverter
    {
        //Konvertiert ein LevelEditorExportData-Objekt, was zuerst noch aus einer Datei als JSON-String gelesen wird, in ein SimulatorInputData-Objekt
        public static SimulatorInputData Convert(string editorFileName)
        {
            var data = JsonHelper.Helper.CreateFromJson<LevelEditorExportData>(FileNameReplacer.LoadEditorFile(editorFileName));

            var panel = new GraphicPanel2D() { Width = 100, Height = 100, Mode = Mode2D.CPU };
            var editor = new EditorViewModel(new EditorInputData() { Panel = panel }, new EditorViewModelActions());
            editor.LoadFromExportObject(data);
            return editor.GetSimulatorExport();
        }

        //Konvertiert PhysikLevelItemExportData nach PrototypControlExportData
        //Wenn man GameSimulator.GetExportDataFromLevelItem aufgerufen hat, kann man dieses LevelItem dann beim
        //LevelEditor per PasteFromClipboard in das PrototypControl einfügen
        public static string ConvertLevelItemExportToPhysicPrototyp(PhysikLevelItemExportData exportData)
        {
            return ConvertLevelItemsExportToPhysicPrototyps(new PhysikLevelItemExportData[] { exportData });
        }

        public static string ConvertLevelItemsExportToPhysicPrototyps(PhysikLevelItemExportData[] exportData)
        {
            var protoExport = new PrototypControlExportData()
            {
                PrototypItems = exportData.Select(x => Convert(x)).ToArray()
            };

            string exportString = JsonHelper.Helper.ToJson(protoExport);

            return exportString;
        }


        private static PhysicItemExportData Convert(PhysikLevelItemExportData exportData)
        {
            return new PhysicItemExportData()
            {
                Id = -1,
                PhysicSceneData = exportData.PhysicSceneData,
                AnimationData = exportData.AnimationData.Select(x => new KeyFrameEditorControl.Controls.KeyFrameEditor.KeyFrameEditorExportData()
                {
                    AnimationData = x
                }).ToArray(),
                TextureData = exportData.TextureData,
            };
        }
    }
}
