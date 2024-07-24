using LevelEditorGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Linq;

namespace LevelEditorControl.Controls.KeyboardMappingControl
{
    internal class KeyboardMappingViewModel : ReactiveObject
    {
        public ObservableCollection<KeyMappingEntryViewModel> Items { get; set; } = new ObservableCollection<KeyMappingEntryViewModel>();

        [Reactive] public KeyMappingEntryViewModel SelectedItem { get; set; }
        public ReactiveCommand<Unit, Unit> AddEntryClick { get; private set; }

        public int LevelItemId { get; private set; }

        public KeyboardMappingViewModel(IKeyboardControlledLevelItem handlerProvider, KeyboardMappingTable startValues)
        {
            this.LevelItemId = handlerProvider.Id;
            this.AddEntryClick = ReactiveCommand.Create(() =>
            {
                this.Items.Add(new KeyMappingEntryViewModel(handlerProvider, (item) => this.Items.Remove(item)));
            });

            if (startValues != null)
            {
                foreach (var value in startValues.Entries)
                {
                    var newItem = new KeyMappingEntryViewModel(handlerProvider, (item) => this.Items.Remove(item));
                    newItem.SelectedKey = value.Key;
                    newItem.SelectedHandlerName = newItem.HandlerNames[value.HandlerId];
                    this.Items.Add(newItem);
                }
            }
        }

        //All die Key-Handler-Mappings für ein einzelnes PhysicLevelItem
        public KeyboardMappingTable GetMappingTable()
        {
            return new KeyboardMappingTable(this.LevelItemId, Items.Select(x => x.GetEntry()).Where(x => x != null).Cast<KeyboardMappingEntry>().ToArray());
        }
    }
}
