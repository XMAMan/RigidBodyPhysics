using DemoApplicationHelper;
using ElmaControl.Controls.Game;
using ElmaControl.Controls.LevelSelect;
using ElmaControl.Controls.MainSelect;
using ElmaControl.Controls.SingleLevel;
using LevelEditorControl;
using SpriteEditorControl;
using System;
using System.Windows.Controls;
using WpfControls.Model;

namespace ElmaControl.Controls.Main
{
    enum ControlType { Nothing, MainSelect, LevelSelect, SingleLevel, Game, LevelEditor, SpriteEditor }

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
                case ControlType.Nothing:
                    return null;

                case ControlType.MainSelect:
                    {
                        var vm = new MainSelectViewModel();
                        var control = new Controls.MainSelect.MainSelectControl() { DataContext = vm };
                        return control;
                    }

                case ControlType.LevelSelect:
                    {
                        var vm = new LevelSelectViewModel(data.DataFolder);
                        var control = new Controls.LevelSelect.LevelSelectControl() { DataContext = vm };
                        return control;
                    }

                case ControlType.SingleLevel:
                    {
                        var vm = new SingleLevelViewModel(data.DataFolder);
                        var control = new Controls.SingleLevel.SingleLevelControl() { DataContext = vm };
                        return control;
                    }

                case ControlType.Game:
                    {
                        var vm = new GameViewModel(data.Panel, (data as EditorInputDataWithSound).SoundGenerator, data.TimerTickRateInMs, data.DataFolder);
                        var control = new Controls.Game.GameControl(vm, data.Panel);
                        return control;
                    }

                case ControlType.LevelEditor:
                    {
                        return new LevelEditorFactory().CreateEditorControl(new EditorInputData(data) { DataFolder = data.DataFolder + "Levels\\", ShowGoBackButton = true });
                    }

                case ControlType.SpriteEditor:
                    {
                        return new SpriteEditorFactory().CreateEditorControl(new EditorInputData(data) { ShowSaveLoadButtons = false, ShowGoBackButton = true });
                    }

            }

            throw new NotImplementedException();
        }
    }
}
