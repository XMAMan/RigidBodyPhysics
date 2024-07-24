namespace LevelEditorGlobal
{
    public class SimulatorInputData
    {
        public PhysikLevelItemExportData[] PhysicLevelItems { get; set; }
        public bool[,] CollisionMatrix { get; set; }        
        public bool HasGravity { get; set; }
        public float Gravity { get; set; }
        public int IterationCount { get; set; }
        public int CameraTrackedLevelItemId { get; set; }
        public CameraTrackerData CameraTrackerData { get; set; }
        public ImageData BackgroundImage { get; set; }
        public BackgroundItemSimulatorExportData[] BackgroundItems { get; set; }

        public SimulatorInputData() { }

        public SimulatorInputData(SimulatorInputData copy)
        {
            this.PhysicLevelItems = new PhysikLevelItemExportData[copy.PhysicLevelItems.Length];
            for (int i=0;i<this.PhysicLevelItems.Length;i++)
            {
                this.PhysicLevelItems[i] = new PhysikLevelItemExportData(copy.PhysicLevelItems[i]);
            }
            this.CollisionMatrix = copy.CollisionMatrix;
            this.HasGravity = copy.HasGravity;
            this.Gravity = copy.Gravity;
            this.IterationCount = copy.IterationCount;
            this.CameraTrackedLevelItemId = copy.CameraTrackedLevelItemId;
            this.CameraTrackerData = new CameraTrackerData(copy.CameraTrackerData);
            this.BackgroundImage = new ImageData(copy.BackgroundImage);
            this.BackgroundItems = new BackgroundItemSimulatorExportData[copy.BackgroundItems.Length];
            for (int i=0;i<copy.BackgroundItems.Length;i++)
            {
                this.BackgroundItems[i] = new BackgroundItemSimulatorExportData(copy.BackgroundItems[i]);
            }
        }
    }
}
