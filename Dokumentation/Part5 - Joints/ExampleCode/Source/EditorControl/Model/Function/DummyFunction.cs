using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using GraphicPanels;

namespace EditorControl.Model.Function
{
    //Hilft dabei, dass all die Funcion-Klassen, die bestimmte Handler nicht haben, diese nicht mit implementieren müssen
    internal abstract class DummyFunction : IFunction
    {
        public abstract IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints);
        public virtual void HandleKeyDown(System.Windows.Input.KeyEventArgs e) { }
        public virtual void HandleKeyUp(System.Windows.Input.KeyEventArgs e) { }
        public virtual void HandleMouseClick(System.Windows.Forms.MouseEventArgs e) { }
        public virtual void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e) { }
        public virtual void HandleMouseMove(System.Windows.Forms.MouseEventArgs e) { }
        public virtual void HandleMouseDown(System.Windows.Forms.MouseEventArgs e) { }
        public virtual void HandleMouseUp(System.Windows.Forms.MouseEventArgs e) { }
        public virtual void Draw(GraphicPanel2D panel) { }
        public abstract FunctionHelpText GetHelpText();
        public virtual void Dispose() { }
    }
}
