using KeyFrameEditorControl.Controls.CheckBox;
using KeyFrameEditorControl.Controls.Slider;
using KeyFrameGlobal;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;


namespace KeyFrameEditorControl.Controls.ControlListBox
{
    //Schreibt die Animations-Propertys sowohl direkt in den selektierten Frame (OutputData) als auch in das AnimationObjekt (Anzeigedaten)
    public class ControlListBoxViewModel : ReactiveObject
    {
        public IEnumerable<IControlListBoxItem> Items { get; set; }

        [Reactive] public IControlListBoxItem SelectedItem { get; set; }

        [Reactive] public FrameData SelectedFrame { get; set; } = null; //Wenn kein Key aktiviert ist steht hier Null. Ansonsten der zugehörige Frame des Keys
        [Reactive] public Visibility Visibility { get; set; } = Visibility.Visible;

        public bool[] IsPropertyAnimated { get; set; } //Legt für jedes Element aus AnimatorInputData.Properties ob während der Animation der Setter beschrieben wird

        public ControlListBoxViewModel(AnimatorInputData animationData)
        {
            this.IsPropertyAnimated = new bool[animationData.Properties.Length];
            for (int i = 0; i < animationData.Properties.Length; i++) this.IsPropertyAnimated[i] = true;

            this.Items = animationData.Properties.Select((x, index) => AnimationPropToListItem(x, index)).ToArray();

            this.WhenAnyValue(x => x.SelectedFrame).Subscribe(frame =>
            {
                if (frame != null)
                {
                    SetFrameData(frame);
                }

                this.Visibility = frame == null ? Visibility.Hidden : Visibility.Visible; //Wenn kein Key selektiert ist, dann zeige nichts an
            });
        }

        private IControlListBoxItem AnimationPropToListItem(IAnimationProperty animationProperty, int index)
        {
            if (animationProperty is IFloatAnimationProperty)
                return new SliderControlViewModel((IFloatAnimationProperty)animationProperty, index, this);

            if (animationProperty is IBoolAnimationProperty)
                return new CheckBoxControlViewModel((IBoolAnimationProperty)animationProperty, index, this);

            throw new ArgumentException("Can not convert " + animationProperty.GetType() + " to IControlListBoxItem");
        }

        public FrameData GetFrameData(float time)
        {
            return new FrameData(time, this.Items.Select(x => x.ObjValue).ToArray());
        }

        private void SetFrameData(FrameData frameData)
        {
            for (int i = 0; i < frameData.Values.Length; i++)
            {
                var item = this.Items.ToList()[i];
                item.ObjValue = frameData.Values[i];
                item.IsAnimated = this.IsPropertyAnimated[i];
            }
        }
    }
}
