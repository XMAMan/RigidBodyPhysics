using GraphicPanels;
using System;
using System.Windows.Controls;

namespace WpfControls.Model
{
    //Der Leveleditor besteht aus lauter Editor-Controls, welche alle als Input ein String bekommen (Kann auch leer sein)
    //und ein String zurück geben.
    //Das ViewModel vom UserControl 
    public interface IEditorFactory
    {
        UserControl CreateEditorControl(EditorInputData data); //Der DataContext vom UserControl kann IGraphicPanelHandler, ITimerHandler oder IStringSerializable implementieren
        object CreateEditorViewModel(EditorInputData data);
    }

    public class EditorInputData
    {
        public GraphicPanel2D Panel; //Es gibt ein globales GraphicPanel2D-Objekt, was von allen Editor-Controls genutzt wird. 
        public float TimerTickRateInMs = float.NaN; //Falls das ViewModel ITimerHandler implementiert ist wird dieser Parameter gebraucht
        public object InputData = null; //Ergebnis von IObjectSerializable.GetExportObject()
        public Action<object> IsFinished; //Jemand hat den Switch/GoBack-Button gedrückt; Parameter: Sender-Viewmodel
        public bool ShowSaveLoadButtons = true;
        public bool ShowGoBackButton = true;
        public string DataFolder = null;

        public EditorInputData() { }
        public EditorInputData(EditorInputData copy)
        {
            this.Panel = copy.Panel;
            this.TimerTickRateInMs = copy.TimerTickRateInMs;
            this.InputData = copy.InputData;
            this.IsFinished = copy.IsFinished;
            this.ShowSaveLoadButtons = copy.ShowSaveLoadButtons;
            this.ShowGoBackButton = copy.ShowGoBackButton;
            this.DataFolder = copy.DataFolder;
        }
    }

    public interface IPhysicSceneEditor
    {
        string PhysicSceneJson { get; } //Bearbeitet diese Klasse ein PhysicScene-Objekt? Wenn ja, ist dass das interne PhysicScene-Objekt als Json
    }
}
