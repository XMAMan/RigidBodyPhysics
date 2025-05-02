using GraphicPanels;
using LevelEditorExports.Editor.BackgroundImage;
using LevelEditorExports.Editor.CameraTracking;
using LevelEditorExports.Editor.KeyboardMappings;
using LevelEditorGlobal;
using PhysicGlobal;

namespace LevelToSimulatorConverter
{
    //Dieses Klasse wird aus dem EditorState-Objekt erzeugt
    public class EditorDataForSimulation
    {
        public List<IPhysicMergerItem> Items;
        public KeyboardMappingTable[] KeyboardMappings;
        public bool[,] CollisionMatrix;
        public float timerIntervalInMilliseconds;
        public ImageData BackgroundImage;
        public string ForegroundImage;
        public bool HasGravity;
        public float Gravity;
        public int IterationCount;

        public int CameraTrackedLevelItemId;
        public GraphicPanel2D Panel;
        public Camera2D Camera; //Diese Kamera wird vom CameraTracker dann in der Simulation gesteuert
        public CameraTrackerData CameraTrackerData;
        public BackgroundItemSimulatorExportData[] BackgroundItems;
        public EditorTagdata[] TagData;
    }
}
