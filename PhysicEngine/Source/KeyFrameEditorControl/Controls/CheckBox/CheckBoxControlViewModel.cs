using KeyFrameGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using KeyFrameEditorControl.Controls.ControlListBox;

namespace KeyFrameEditorControl.Controls.CheckBox
{
    internal class CheckBoxControlViewModel : ReactiveObject, IControlListBoxItem
    {
        private IBoolAnimationProperty prop;
        public bool IsEnabled
        {
            get
            {
                return prop.Value;
            }
            set
            {
                this.prop.Value = value;    //Schreibe Wert von der View auf das angezeigte Animationobjekt
                this.parent.SelectedFrame.Values[this.Index] = value;//Schreibe den Wert auch auf den selektierten Frame
                this.RaisePropertyChanged(nameof(IsEnabled));
            }
        }

        public object ObjValue
        {
            get
            {
                return this.IsEnabled;
            }
            set
            {
                this.IsEnabled = (bool)value;
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

        [Reactive] public int Index { get; set; }


        private ControlListBoxViewModel parent;

        public CheckBoxControlViewModel(IBoolAnimationProperty animationProperty, int index, ControlListBoxViewModel parent)
        {
            this.prop = animationProperty;
            this.Index = index;
            this.parent = parent;
        }
    }
}
