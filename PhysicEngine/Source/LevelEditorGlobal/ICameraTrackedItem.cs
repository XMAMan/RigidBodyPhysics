namespace LevelEditorGlobal
{
    public interface ICameraTrackedItem
    {
        RectangleF BoundingBox { get; } //Boundingbox von den Objekt, welche immer im Sichtbereich der Kamera bleiben soll
    }
}
