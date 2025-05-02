using PhysicGlobal;

namespace LevelEditorExports.Editor.Tagging
{
    //Wird vom Editor zum speichern der Tagdaten benutzt. 
    public class TagEditorData
    {
        public string Id { get; set; } //TreeItem.Title
        public string Name { get; set; } = string.Empty;
        public byte Color { get; set; } = 0;
        public List<Vec2D> AnchorPoints { get; set; } = new List<Vec2D>();

        public bool HasData()
        {
            return this.Name != string.Empty || this.Color != 0 || this.AnchorPoints.Any();
        }

        public TagEditorData(string id)
        {
            this.Id = id;
        }
    }
}
