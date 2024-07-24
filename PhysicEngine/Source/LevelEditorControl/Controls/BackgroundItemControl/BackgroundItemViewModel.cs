using LevelEditorControl.LevelItems.BackgroundItem;
using ReactiveUI;

namespace LevelEditorControl.Controls.BackgroundItemControl
{
    internal class BackgroundItemViewModel : ReactiveObject
    {
        private BackgroundPrototypItem model;

        public float ZValue { get => this.model.ZValue; set => this.model.ZValue = value; }
        //[Reactive] public float Bias { get; set; } = 40;

        public BackgroundItemViewModel(BackgroundPrototypItem model)
        {
            this.model = model;
        }
    }
}
