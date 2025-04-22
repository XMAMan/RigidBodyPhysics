namespace LevelEditorGlobal
{
    public class CameraTrackerData
    {
        public enum TrackingMode { KeepAwayFromBorder, KeepInCenter}

        public bool IsActive = true;                // Soll der CameraTracker die Kameraposition verändern?
        public TrackingMode Mode = TrackingMode.KeepAwayFromBorder;
        public float DistanceToScreenBorder = 50;   // So viele CameraSpace-Einheiten hat der Tracking-Punkt immer zum Bildschirmrand Platz wenn Mode==KeepAwayFromBorder
        public float DistanceToScreenCenter = 50;   // So viele CameraSpace-Einheiten hat der Tracking-Punkt immer zur Bildschirmmitte Platz wenn Mode==KeepInCenter
        public float CameraZoom = 1;                // Dieser Zoom wird eingestellt, wenn der Tracker aktiv ist
        public RectangleF? MaxBorder = null;        // Nur innerhalb von diesen Rechteck darf sich das Kamerafenster bewegen
        public float SpringConstant = 0.001f;       // Federkonstante welche die Kamera beschleunigt
        public float AirFriction = 0.005f;          // Reibungsfaktor, welcher die Kamerabewegung bremst

        public CameraTrackerData() { }

        public CameraTrackerData(CameraTrackerData data)
        {
            IsActive = data.IsActive;
            Mode = data.Mode;
            DistanceToScreenBorder = data.DistanceToScreenBorder;
            DistanceToScreenCenter = data.DistanceToScreenCenter;
            CameraZoom = data.CameraZoom;
            MaxBorder = data.MaxBorder;
            SpringConstant = data.SpringConstant;
            AirFriction = data.AirFriction;
        }
    }
}
