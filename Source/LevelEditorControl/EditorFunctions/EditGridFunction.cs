using LevelEditorControl.Controls.GridModeControl;
using System;
using System.Windows.Forms;

namespace LevelEditorControl.EditorFunctions
{
    internal class EditGridFunction : DummyFunction, IEditorFunction
    {
        private EditorState state;
        private Action<MouseEventArgs?> isFinish;

        public override FunctionType Type => FunctionType.EditGrid;

        public override IEditorFunction Init(EditorState state)
        {
            throw new NotImplementedException();
        }

        public IEditorFunction Init(EditorState state, Action<MouseEventArgs?> isFinish)
        {
            this.state = state;
            this.isFinish = isFinish;
            this.HasPropertyControl = true;
            return this;
        }

        public override System.Windows.Controls.UserControl GetPropertyControl()
        {
            var vm = new GridModeViewModel(this.state.Grid);

            return new GridModeControl() { DataContext = vm };
        }

        public override void HandleTimerTick(float dt)
        {
            var panel = this.state.Panel;

            state.DrawItems();
            panel.FlipBuffer();
        }


        public override void HandleMouseClick(MouseEventArgs e)
        {
            this.isFinish(e);
        }
    }
}
