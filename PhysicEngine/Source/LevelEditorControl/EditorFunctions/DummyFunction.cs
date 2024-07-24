using System;

namespace LevelEditorControl.EditorFunctions
{
    internal abstract class DummyFunction : IEditorFunction
    {
        public abstract FunctionType Type { get; }
        public abstract IEditorFunction Init(EditorState state);
        public bool HasPropertyControl { get; protected set; } = false;
        public virtual System.Windows.Controls.UserControl GetPropertyControl() { throw new NotImplementedException(); }
        public virtual void HandleTimerTick(float dt) { }
        public virtual void HandleKeyDown(System.Windows.Input.KeyEventArgs e) { }
        public virtual void HandleKeyUp(System.Windows.Input.KeyEventArgs e) { }
        public virtual void HandleMouseClick(System.Windows.Forms.MouseEventArgs e) { }
        public virtual void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e) { }
        public virtual void HandleMouseMove(System.Windows.Forms.MouseEventArgs e) { }
        public virtual void HandleMouseDown(System.Windows.Forms.MouseEventArgs e) { }
        public virtual void HandleMouseUp(System.Windows.Forms.MouseEventArgs e) { }
        public virtual void HandleMouseEnter() { }
        public virtual void HandleMouseLeave() { }
        public virtual void HandleSizeChanged(int width, int height) { }
        public virtual void Dispose() { }
    }
}
