using GraphicMinimal;

namespace LevelEditorGlobal
{
    public class PhysicSceneTagdataEntry
    {
        //TagID aus Sicht des Editors: LevelItemID+TagType+ITagable.Id
        //TagID aus Sicht des Simulators: TagType+ITagable.Id
        public ITagable.TagType TagType { get; }
        public int LevelItemId { get; } //ILevelItem.Id
        public int TagId { get; } //ITagable.Id = Index aus RuntimeLevelItem.Bodies/Joints/Thrusters/Motors
        public string[] Names { get; }
        public byte Color { get; }
        public Vector2D[] AnchorPoints { get; }

        public PhysicSceneTagdataEntry(ITagable.TagType tagType, int levelItemId, int tagId, string[] names, byte color, Vector2D[] anchorPoints)
        {
            this.TagType = tagType;
            this.LevelItemId = levelItemId;
            this.TagId = tagId;
            this.Names = names;
            this.Color = color;
            this.AnchorPoints = anchorPoints;
        }

        public PhysicSceneTagdataEntry(PhysicSceneTagdataEntry copy)
        {
            this.TagType = copy.TagType;
            this.LevelItemId = copy.LevelItemId;
            this.TagId = copy.TagId;
            this.Names = copy.Names.ToArray();
            this.Color = copy.Color;
            this.AnchorPoints = copy.AnchorPoints.Select(x => new Vector2D(x)).ToArray();
        }

        public PhysicSceneTagdataEntry(PhysicSceneTagdataEntry copy, int levelItemId)
            :this(copy)
        {
            this.LevelItemId = levelItemId;
        }
    }
}
