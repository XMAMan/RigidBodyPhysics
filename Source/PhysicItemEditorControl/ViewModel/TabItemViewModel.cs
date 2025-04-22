using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Reactive;

namespace PhysicItemEditorControl.ViewModel
{
    public class TabItemViewModel : ReactiveObject
    {
        public enum TabItemType { PhysicEditor, TextureEditor, AnimationEditor }

        public string Header { get; set; }
        public TabItemType Type { get; set; }
        [Reactive] public System.Windows.Controls.UserControl Content { get; set; }

        public ReactiveCommand<Unit, Unit> DeleteTabClick { get; private set; }

        public TabItemViewModel(string header, TabItemType type, System.Windows.Controls.UserControl content, Action<TabItemViewModel> deleteTabAction)
        {
            Header = header;
            Type = type;
            Content = content;

            this.DeleteTabClick = ReactiveCommand.Create(() =>
            {
                deleteTabAction(this);
            });
        }
    }
}
