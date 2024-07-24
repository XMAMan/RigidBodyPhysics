using KeyFrameGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using KeyFrameEditorControl.Controls.ControlListBox;

namespace KeyFrameEditorControl.Controls.Slider
{
    internal class SliderControlViewModel : ReactiveObject, IControlListBoxItem
    {
        private IFloatAnimationProperty prop;
        public float Value
        {
            get
            {
                return this.prop.Value;
            }
            set
            {
                this.prop.Value = value;    //Schreibe Wert von der View auf das angezeigte Animationobjekt
                this.parent.SelectedFrame.Values[this.Index] = value;//Schreibe den Wert auch auf den selektierten Frame
                this.RaisePropertyChanged(nameof(Value));
            }
        }

        public object ObjValue
        {
            get
            {
                return this.Value;
            }
            set
            {
                this.Value = (float)value;
            }
        }

        public bool IsAnimated
        {
            get => parent.IsPropertyAnimated[Index];
            set
            {
                parent.IsPropertyAnimated[Index] = value;
                this.RaisePropertyChanged(nameof(IsAnimated));
            }
        }

        [Reactive] public float MinValue { get; set; } = 0;
        [Reactive] public float MaxValue { get; set; } = 1;
        [Reactive] public int Index { get; set; }



        private ControlListBoxViewModel parent;

        public SliderControlViewModel(IFloatAnimationProperty animationProp, int index, ControlListBoxViewModel parent)
        {
            this.prop = animationProp;
            this.MinValue = animationProp.MinValue;
            this.MaxValue = animationProp.MaxValue;
            this.Index = index;
            this.parent = parent;
        }
    }
}
