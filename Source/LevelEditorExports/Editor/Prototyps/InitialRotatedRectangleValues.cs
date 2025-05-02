using PhysicGlobal;

namespace LevelEditorExports.Editor.Prototyps
{
    public class InitialRotatedRectangleValues
    {
        public float SizeFactor { get; set; } = 1;//Mit dem SizeFactor werden LevelItems von diesen Prototyp angelegt
        public float AngleInDegree { get; set; } = 0;
        public Vec2D LocalPivot { get; set; } = new Vec2D(0, 0);
    }
}
