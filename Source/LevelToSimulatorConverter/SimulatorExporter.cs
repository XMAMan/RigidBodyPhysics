using LevelEditorExports.Editor;
using LevelEditorExports.Editor.BackgroundImage;
using LevelEditorExports.Editor.Helper;
using LevelEditorExports.Editor.KeyboardMappings;
using LevelEditorExports.Simulator;
using LevelToSimulatorConverter._1_LokalToWorldTransform;
using LevelToSimulatorConverter._2_MergeToSingleScene;
using LevelToSimulatorConverter._3_Tagging;

namespace LevelToSimulatorConverter
{
    //Mögliche Konvertierungswege:
    //LevelEditorExportData -> EditorState -> EditorDataForSimulation -> SimulatorInputData
    //LevelEditorExportData -> SimulatorInputData

    //Konvertiert ein EditorDataForSimulation- oder LevelEditorExportData-Objekt in ein SimulatorInputData-Objekt
    public static class SimulatorExporter
    {
        public static SimulatorInputData Convert(string editorFileName)
        {
            var data = JsonHelper.Helper.CreateFromJson<LevelEditorExportData>(FileNameReplacer.LoadEditorFile(editorFileName));
            return Convert(data);
        }

        public static SimulatorInputData Convert(LevelEditorExportData levelExport)
        {
            //Schritt 1: Konvertiere die Physikitems von lokalen Koordinaten in Weltkoordinaten
            var physicItems = PhysicExportToMergerItemConverter.GetPhysicLevelItemsFromExportScene(levelExport).ToList();            
            var keyboardMappings = levelExport.KeyboardMappingTables != null ? levelExport.KeyboardMappingTables : new KeyboardMappingTable[0];   
            
            //Schritt 2: Konvertiere die Tagdaten
            var tagData = TagDataConverter.Convert(levelExport.TagData, levelExport.Prototyps, levelExport.LevelItems);

            //Schritt 3: Merge alle Physikitems zu einer Physikscene zusammen
            var physicLevelItems = PhysicItemConverter.Convert(physicItems, levelExport.BackgroundImage, levelExport.ForegroundImage, keyboardMappings, tagData);

            //Schritt 4: Konvertiere die Backgrounditems von lokalen Koordinaten in Weltkoordinaten
            var backgroundItems = BackgroundExportToSimulatorConverter.GetBackgroundItemsFromExportScene(levelExport);


            var collisionMatrix = levelExport.CollisionMatrix != null ? levelExport.CollisionMatrix : new bool[5, 5];
            var backgroundImage = new ImageData() { FileName = levelExport.BackgroundImage, Mode = levelExport.BackgroundImageMode };

            return new SimulatorInputData()
            {
                PhysicLevelItems = physicLevelItems,
                CollisionMatrix = collisionMatrix,
                HasGravity = levelExport.HasGravity,
                Gravity = levelExport.Gravity,
                IterationCount = levelExport.SimulatorIterationCount,
                CameraTrackedLevelItemId = levelExport.CameraTrackedLevelItemId,
                CameraTrackerData = levelExport.CameraTrackerData,
                BackgroundImage = backgroundImage,
                BackgroundItems = backgroundItems,
            };
        }

        public static SimulatorInputData Convert(EditorDataForSimulation data)
        {
            var physicLevelItems = PhysicItemConverter.Convert(data.Items, data.BackgroundImage.FileName, data.ForegroundImage, data.KeyboardMappings, data.TagData);
            
            return new SimulatorInputData()
            {
                PhysicLevelItems = physicLevelItems,
                CollisionMatrix = data.CollisionMatrix,
                HasGravity = data.HasGravity,
                Gravity = data.Gravity,
                IterationCount = data.IterationCount,
                CameraTrackedLevelItemId = data.CameraTrackedLevelItemId,
                CameraTrackerData = data.CameraTrackerData,
                BackgroundImage = data.BackgroundImage,
                BackgroundItems = data.BackgroundItems,          
            };
        }
    }
}
