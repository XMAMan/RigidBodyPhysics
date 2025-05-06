namespace Simulator.CameraTracking
{
    public interface ICameraTrackedItem
    {
        PhysicGlobal.BoundingBox BoundingBox { get; } //Boundingbox von den Objekt, welche immer im Sichtbereich der Kamera bleiben soll
    }
}
