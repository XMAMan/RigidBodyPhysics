using PhysicGlobal;

namespace LevelEditorExports.Editor.LevelItems
{
    public class PolygonLevelItemExportData : ILevelItemExportData
    {
        public int LevelItemId { get; set; }
        public Vec2D[] Points { get; set; }
        public float Friction { get; set; }
        public float Restiution { get; set; }
        public int CollisionCategory { get; set; }
    }
}
