using GraphicMinimal;
using System.Collections.Generic;
using System.Linq;

namespace LevelEditorControl.Controls.TagItemControl
{
    //Wird vom Editor zum speichern der Tagdaten benutzt. 
    internal class TagEditorData
    {
        public string Id { get; set; } //TreeItem.Title
        public string Name { get; set; } = string.Empty;
        public byte Color { get; set; } = 0;
        public List<Vector2D> AnchorPoints { get; set; } = new List<Vector2D>();

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
