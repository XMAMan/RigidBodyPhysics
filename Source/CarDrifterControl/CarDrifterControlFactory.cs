using CarDrifterControl.Controls;
using DemoApplicationHelper;
using LevelEditorControl;
using Simulator;
using WpfControls.Model;

namespace CarDrifterControl
{
    //Möglichkeit 1: Spiel wird direkt ohne Leveleditor gestartet
    public class CarDrifterControlFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = (MainViewModel)CreateEditorViewModel(data);
            var editorControl = new Controls.MainControl(vm, data.Panel);
            return editorControl;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new MainViewModel(data.Panel, (data as EditorInputDataWithSound).SoundGenerator, data.TimerTickRateInMs, data.DataFolder); ;
        }
    }

    //Möglichkeit 2: Es wird der Leveleditor gestartet, welcher den CarDrifterSimulator nutzt
    public class CarDrifterWithLevelEditorControlFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = (MainViewModel)CreateEditorViewModel(data);
            var levelEditorControl = new LevelEditorFactory().CreateEditorControl(data);
            (levelEditorControl.DataContext as IToTextWriteable).LoadFromTextFile(data.DataFolder + "CarDrifter.txt"); //Level einnladen
            (levelEditorControl.DataContext as ISimlatorUser).SetSimulatorBuildMethod(
                        (data, panelSize, camera, timerIntercallInMilliseconds) => (vm).CreateSimulator(data, panelSize, camera, timerIntercallInMilliseconds)
                        );

            return levelEditorControl;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new MainViewModel(data.Panel, (data as EditorInputDataWithSound).SoundGenerator, data.TimerTickRateInMs, data.DataFolder); //Game-ViewModel
        }
    }
}
