using GraphicPanelWpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Windows.Input;

namespace ElmaControl.Controls.MainSelect
{
    internal class MainSelectViewModel : ReactiveObject, IKeyDownUpHandler
    {
        public enum SelectedEntry { LevelSelect, Leveleditor, SpriteEditor, Quit }

        [Reactive] public int SelectedIndex { get; set; } = 0;

        public event Action<SelectedEntry> SelectedEntryChanged;

        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                SelectedIndex--;
                if (SelectedIndex < 0) SelectedIndex = 0;
            }
            if (e.Key == Key.Down)
            {
                SelectedIndex++;
                if (SelectedIndex > 3) SelectedIndex = 3;
            }

            if (e.Key == Key.Enter)
            {
                var entry = (SelectedEntry)SelectedIndex;
                SelectedEntryChanged?.Invoke(entry);
            }

            if (e.Key == Key.Escape)
            {
                SelectedEntryChanged?.Invoke(SelectedEntry.Quit);
            }
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
        }
    }
}
