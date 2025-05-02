using PhysicGlobal;

namespace LevelEditorExports.Simulator
{
    //Bekommt der SimulatorExporter als Input
    public class EditorTagdata
    {
        public int LevelItemId { get; set; }    //ILevelItem.Id
        public int TagId { get; set; }          //ITagable.Id
        public TagType TagType { get; set; } //ITagable.TypeName
        public string PrototypTagName { get; set; } = string.Empty;
        public byte PrototypColor { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public byte Color { get; set; } = 0;
        public Vec2D[] AnchorPoints { get; set; }
    }
}
