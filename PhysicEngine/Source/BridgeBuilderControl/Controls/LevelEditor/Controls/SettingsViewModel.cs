using BridgeBuilderControl.Controls.LevelEditor.Functions;
using ReactiveUI;

namespace BridgeBuilderControl.Controls.LevelEditor.Controls
{
    //Beschreibt aus dem EditorState die Propertys: XCount, YCount, GroundHeight, WaterHeight, Budget
    internal class SettingsViewModel : ReactiveObject
    {
        public uint XCount
        {
            get => this.state.XCount;
            set
            {
                this.state.XCount = value;
                this.RaisePropertyChanged(nameof(XCount));
            }
        }
        public uint YCount
        {
            get => this.state.YCount;
            set
            {
                this.state.YCount = value;
                this.RaisePropertyChanged(nameof(YCount));
            }
        }
        public uint GroundHeight
        {
            get => this.state.GroundHeight;
            set
            {
                this.state.GroundHeight = value;
                this.RaisePropertyChanged(nameof(GroundHeight));
            }
        }
        public uint WaterHeight
        {
            get => this.state.WaterHeight;
            set
            {
                this.state.WaterHeight = value;
                this.RaisePropertyChanged(nameof(WaterHeight));
            }
        }
        public uint Budget
        {
            get => this.state.Budget;
            set
            {
                this.state.Budget = value;
                this.RaisePropertyChanged(nameof(Budget));
            }
        }

        public float TrainExtraSpeed
        {
            get => this.state.TrainExtraSpeed;
            set
            {
                this.state.TrainExtraSpeed = value;
                this.RaisePropertyChanged(nameof(TrainExtraSpeed));
            }
        }

        private EditorState state;
        public SettingsViewModel(EditorState state)
        {
            this.state = state;
        }
    }
}
