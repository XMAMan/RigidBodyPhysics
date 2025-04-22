using GraphicMinimal;
using LevelEditorGlobal;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LevelEditorControl.EditorFunctions
{
    internal class DefineCollisionMatrixFunction : DummyFunction, IEditorFunction
    {
        private EditorState state;
        private ICollidable[] collidables;
        private ICollidable selectedItem = null;

        public override FunctionType Type => FunctionType.DefineCollisionMatrix;

        public override IEditorFunction Init(EditorState state)
        {
            this.state = state;

            List<ICollidable> list = new List<ICollidable>();

            list.AddRange(
                this.state.LevelItems
                .Where(x => x is ICollidable)
                .Cast<ICollidable>());

            list.AddRange(
                this.state.LevelItems
                    .Where(x => x is ICollidableContainer)
                    .Cast<ICollidableContainer>()
                    .SelectMany(x => x.Collidables));

            this.collidables = list.ToArray();

            return this;
        }

        private ICollidable GetSelectedItem(Vector2D point)
        {
            var screenToCamera = this.state.Camera.GetPointToCameraMatrix();

            List<KeyValuePair<float, ICollidable>> list = new List<KeyValuePair<float, ICollidable>>();
            foreach (var item in this.collidables)
            {
                if (item.IsPointInside(point, screenToCamera))
                {
                    list.Add(new KeyValuePair<float, ICollidable>(item.GetArea(), item));
                }
            }

            if (list.Any())
                return list.OrderBy(x => x.Key).First().Value;

            return null;
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            this.selectedItem = GetSelectedItem(new Vector2D(e.X, e.Y));

            Refresh();
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            this.selectedItem = GetSelectedItem(new Vector2D(e.X, e.Y));

            if (this.selectedItem != null)
            {
                this.selectedItem.CollisionCategory = this.state.CollisionMatrixViewModel.SelectedIndex;
            }

            Refresh();
        }

        public override void HandleTimerTick(float dt)
        {
            Refresh();
        }

        private void Refresh()
        {
            var panel = this.state.Panel;

            panel.ClearScreen(Color.White);

            panel.MultTransformationMatrix(this.state.Camera.GetPointToSceenMatrix());

            foreach (var item in this.collidables)
            {
                var color = this.state.CollisionMatrixViewModel.Colors[item.CollisionCategory];
                item.DrawBorder(panel, new Pen(color, item == this.selectedItem ? 5 : 3));
            }

            panel.FlipBuffer();
        }
    }
}
