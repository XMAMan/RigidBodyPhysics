using LevelEditorControl.LevelItems;
using LevelEditorGlobal;
using PhysicGlobal;
using System;
using System.Drawing;
using System.Windows.Forms;
using WpfControls.Extensions;

namespace LevelEditorControl.EditorFunctions
{
    //Bekommt als Input ein selektiertes IPrototypItem aus dem PrototypUserControl und erzeugt ein neues ILevelItem
    internal class AddItemFunction : DummyFunction, IEditorFunction
    {
        private EditorState state;
        private IPrototypItem prototypItem;
        private Action isFinish;

        public override FunctionType Type { get; } = FunctionType.AddItem;
        public override IEditorFunction Init(EditorState state)
        {
            throw new NotImplementedException();
        }

        public IEditorFunction Init(EditorState state, Action isFinish)
        {
            this.state = state;
            this.prototypItem = state.SelectedPrototyp;
            this.isFinish = isFinish;

            return this;
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            var point = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToPhx();
            point = state.Grid.SnapMouse(point);
            Draw(point);
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            var mousePosition = state.Camera.PointToCamera(new PointF(e.X, e.Y)).ToPhx();
            mousePosition = state.Grid.SnapMouse(mousePosition);

            if (e.Button == MouseButtons.Left)
            {
                int newId = this.state.GenerateLevelItemId();
                this.state.AddLevelItem(LevelItemsHelper.BuildFromPrototyp(this.prototypItem, mousePosition, newId));
            }

            if (e.Button == MouseButtons.Right)
            {
                this.isFinish.Invoke();
            }

        }

        private void Draw(Vec2D mousePosition)
        {
            var panel = this.state.Panel;

            state.DrawItems();

            panel.PushMatrix();
            panel.MultTransformationMatrix(new RotatedRectangle(mousePosition, this.prototypItem.BoundingBox.Size, this.prototypItem.InitialRecValues).GetLocalToScreenMatrix());
            this.prototypItem.Draw(panel);
            panel.PopMatrix();

            panel.FlipBuffer();
        }
    }
}
