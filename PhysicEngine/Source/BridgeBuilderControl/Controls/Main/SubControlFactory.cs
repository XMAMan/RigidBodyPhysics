using BridgeBuilderControl.Controls.BridgeEditor;
using BridgeBuilderControl.Controls.LevelEditor;
using BridgeBuilderControl.Controls.LevelSelect;
using BridgeBuilderControl.Controls.MainSelect;
using BridgeBuilderControl.Controls.OpenDialog;
using BridgeBuilderControl.Controls.Readme;
using BridgeBuilderControl.Controls.SaveDialog;
using BridgeBuilderControl.Controls.Simulator;
using System;
using System.Windows.Controls;
using WpfControls.Model;

namespace BridgeBuilderControl.Controls.Main
{
    enum ControlType { Nothing, MainSelect, LevelSelect, LevelEditor, BridgeEditor, Simulator, SaveDialog, OpenDialog, Readme }

    internal class SubControlFactory
    {
        private EditorInputData data;
        public SubControlFactory(EditorInputData data)
        {
            this.data = data;
        }

        public UserControl CreateControl(ControlType controlType)
        {
            switch (controlType)
            {
                case ControlType.MainSelect:
                    {
                        var vm = new MainSelectViewModel();
                        var control = new Controls.MainSelect.MainSelectControl() { DataContext = vm };
                        return control;
                    }

                case ControlType.LevelSelect:
                    {
                        var vm = new LevelSelectViewModel(this.data.DataFolder);
                        var control = new Controls.LevelSelect.LevelSelectControl() { DataContext = vm };
                        return control;
                    }

                case ControlType.LevelEditor:
                    {
                        var vm = new LevelEditorViewModel(data.Panel, data.DataFolder);
                        var control = new Controls.LevelEditor.LevelEditorControl(vm, data.Panel);
                        return control;
                    }

                case ControlType.BridgeEditor:
                    {
                        var vm = new BridgeEditorViewModel(data.Panel, data.DataFolder);
                        var control = new Controls.BridgeEditor.BridgeEditorControl(vm, data.Panel);
                        return control;
                    }

                case ControlType.Simulator:
                    {
                        var vm = new SimulatorViewModel(data.Panel, data.DataFolder, data.TimerTickRateInMs);
                        var control = new Controls.Simulator.SimulatorControl(vm, data.Panel);
                        return control;
                    }

                case ControlType.SaveDialog:
                    {
                        var vm = new SaveDialogViewModel();
                        var control = new Controls.SaveDialog.SaveDialogControl() { DataContext = vm };
                        return control;
                    }

                case ControlType.OpenDialog:
                    {
                        var vm = new OpenDialogViewModel();
                        var control = new Controls.OpenDialog.OpenDialogControl() { DataContext = vm };
                        return control;
                    }

                case ControlType.Readme:
                    {
                        var vm = new ReadmeViewModel();
                        var control = new Controls.Readme.ReadmeControl() { DataContext = vm };
                        return control;
                    }
            }

            throw new NotImplementedException();
        }
    }
}
