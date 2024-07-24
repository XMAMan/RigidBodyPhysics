using LevelEditorControl.Controls.PrototypControl;
using LevelEditorControl.Controls.TagItemControl;
using LevelEditorGlobal;
using WpfControls.Controls.CameraSetting;

namespace LevelEditorControl.Controls.EditorControl
{
    internal class LevelEditorExportData
    {
        public PrototypControlExportData Prototyps { get; set; }
        public object[] LevelItems { get; set; }
        public string BackgroundImage { get; set; }
        public string ForegroundImage { get; set; }
        public ImageMode BackgroundImageMode { get; set; }
        public bool UseCameraAutoZoom { get; set; }
        public Camera2D.InitialPositionIfAutoZoomIsActivated InitialCameraPosition { get; set; }
        public bool HasGravity { get; set; }
        public int SimulatorIterationCount { get; set; }
        public float Gravity { get; set; }
        public KeyboardMappingTable[] KeyboardMappingTables { get; set; }
        public bool ShowGrid { get; set; }
        public uint GridSize { get; set; }
        public bool[,] CollisionMatrix { get; set; }
        public int CameraTrackedLevelItemId { get; set; }
        public CameraTrackerData CameraTrackerData { get; set; }
        public TagEditorDataExport TagData { get; set; }
    }
}
