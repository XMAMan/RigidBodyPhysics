using GraphicPanels;

namespace EditorControl.Model.Function
{
    //Objekt, was mit Klick auf ein Button aus dem ActionSelectControl erzeugt wird und dann Maus+Tasturereignisse verarbeitet, was den
    //internen Zustand des Objektes verändert. Es modifziert dabei die IElement-Liste und lebt bis der Finish-Zustand erreicht ist
    internal interface IFunction : IDisposable
    {
        IFunction Init(FunctionData functionData);
        void HandleKeyDown(System.Windows.Input.KeyEventArgs e);
        void HandleKeyUp(System.Windows.Input.KeyEventArgs e);
        void HandleMouseClick(System.Windows.Forms.MouseEventArgs e);
        void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e);
        void HandleMouseMove(System.Windows.Forms.MouseEventArgs e);
        void HandleMouseDown(System.Windows.Forms.MouseEventArgs e);
        void HandleMouseUp(System.Windows.Forms.MouseEventArgs e);
        void Draw(GraphicPanel2D panel);
        FunctionHelpText GetHelpText();
    }

    internal struct FunctionHelpText
    {
        public string Headline;
        public string[] Values;
    }
}
