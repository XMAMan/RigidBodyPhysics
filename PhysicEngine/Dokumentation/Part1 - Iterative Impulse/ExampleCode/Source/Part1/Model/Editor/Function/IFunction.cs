using GraphicPanels;
using Part1.Model.Editor.EditorShape;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Part1.Model.Editor.Function
{
    //Objekt, was mit Klick auf ein Button aus dem ActionSelectControl erzeugt wird und dann Maus+Tasturereignisse verarbeitet, was den
    //internen Zustand des Objektes verändert. Es modifziert dabei die IElement-Liste und lebt bis der Finish-Zustand erreicht ist
    interface IFunction : IDisposable
    {
        IFunction Init(List<IEditorShape> shapes);
        void HandleKeyDown(KeyEventArgs e);
        void HandleKeyUp(KeyEventArgs e);
        void HandleMouseClick(System.Windows.Forms.MouseEventArgs e);
        void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e);
        void HandleMouseMove(System.Windows.Forms.MouseEventArgs e);
        void HandleMouseDown(System.Windows.Forms.MouseEventArgs e);
        void HandleMouseUp(System.Windows.Forms.MouseEventArgs e);
        void Draw(GraphicPanel2D panel);
        FunctionHelpText GetHelpText();
    }

    struct FunctionHelpText
    {
        public string Headline;
        public string[] Values;
    }
}
