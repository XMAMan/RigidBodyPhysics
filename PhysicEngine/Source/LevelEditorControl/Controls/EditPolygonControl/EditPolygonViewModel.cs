using LevelEditorControl.LevelItems.Polygon;
using ReactiveUI;

namespace LevelEditorControl.Controls.EditPolygonControl
{
    internal class EditPolygonViewModel : ReactiveObject
    {
        private IEditablePolygon model;

        public float Friction { get => this.model.Friction; set => this.model.Friction = value; }
        public float Restiution { get => this.model.Restiution; set => this.model.Restiution = value; }
        public int CollisionCategory { get => this.model.CollisionCategory; set => this.model.CollisionCategory = value; }

        public EditPolygonViewModel(IEditablePolygon model)
        {
            this.model = model;
        }
    }
}
