using LevelEditorGlobal;

namespace LevelToSimulatorConverter
{
    //Konvertiert ein EditorDataForSimulation-Objekt in ein SimulatorInputData-Objekt
    public static class SimulatorExporter
    {
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
