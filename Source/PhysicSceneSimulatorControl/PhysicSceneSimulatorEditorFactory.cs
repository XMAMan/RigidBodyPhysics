using PhysicSceneSimulatorControl.Controls.Simulator.ViewModel;
using WpfControls.Model;

namespace PhysicSceneSimulatorControl
{
    public class PhysicSceneSimulatorEditorFactory : IEditorFactory
    {
        public System.Windows.Controls.UserControl CreateEditorControl(EditorInputData data)
        {
            var vm = (SimulatorViewModel)CreateEditorViewModel(data);
            var simulatorControl = new Controls.Simulator.View.SimulatorControl(vm, data.Panel);

            if (data.InputData != null)
                vm.LoadFromExportObject(data.InputData);

            return simulatorControl;
        }

        public object CreateEditorViewModel(EditorInputData data)
        {
            return new SimulatorViewModel(data);
        }
    }
}
