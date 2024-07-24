using SimulatorControl.ViewModel;
using WpfControls.Model;

namespace SimulatorControl
{
    public class PhysicSceneSimulatorEditorFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = new SimulatorViewModel(data.Panel, data.TimerTickRateInMs, data.ShowSaveLoadButtons);
            vm.SwitchClick.Subscribe(value => { data.IsFinished(); });
            var simulatorControl = new SimulatorControl.View.SimulatorControl(vm, data.Panel);

            if (data.InputData != null)
                vm.LoadFromExportObject(data.InputData);

            return simulatorControl;
        }
    }
}
