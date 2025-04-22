using ReactiveUI.Fody.Helpers;
using ReactiveUI;

namespace SpriteEditorControl.Controls.Main
{
    internal class TabItemViewModel : ReactiveObject
    {
        public enum TabItemType { PhysicEditor, SpriteEditor }

        public string Header { get; set; }
        public TabItemType Type { get; set; }
        [Reactive] public System.Windows.Controls.UserControl Content { get; set; }


        public TabItemViewModel(string header, TabItemType tabItemType, System.Windows.Controls.UserControl content)
        {
            Header = header;
            Type = tabItemType;
            Content = content;
        }
    }
}
