using LevelEditorGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System;
using System.Linq;
using System.Collections.Generic;

namespace LevelEditorControl.Controls.KeyboardMappingControl
{
    internal class KeyMappingEntryViewModel : ReactiveObject
    {
        public List<System.Windows.Input.Key> Keys { get; set; } = new List<System.Windows.Input.Key>()
        {
            System.Windows.Input.Key.Left,
            System.Windows.Input.Key.Right,
            System.Windows.Input.Key.Up,
            System.Windows.Input.Key.Down,
            System.Windows.Input.Key.Space,
            System.Windows.Input.Key.Q,
            System.Windows.Input.Key.W,
            System.Windows.Input.Key.E,
            System.Windows.Input.Key.R,
            System.Windows.Input.Key.T,
            System.Windows.Input.Key.Z,
            System.Windows.Input.Key.A,
            System.Windows.Input.Key.S,
            System.Windows.Input.Key.D,
            System.Windows.Input.Key.F,
            System.Windows.Input.Key.G,
            System.Windows.Input.Key.H,
            System.Windows.Input.Key.Y,
            System.Windows.Input.Key.X,
            System.Windows.Input.Key.C,
            System.Windows.Input.Key.V,
            System.Windows.Input.Key.B,
            System.Windows.Input.Key.N,
            System.Windows.Input.Key.NumPad1,
            System.Windows.Input.Key.NumPad2,
            System.Windows.Input.Key.NumPad3,
            System.Windows.Input.Key.NumPad4,
            System.Windows.Input.Key.NumPad5,
            System.Windows.Input.Key.NumPad6,
            System.Windows.Input.Key.NumPad7,
            System.Windows.Input.Key.NumPad8,
            System.Windows.Input.Key.NumPad9,
        };

        [Reactive] public System.Windows.Input.Key SelectedKey { get; set; } = System.Windows.Input.Key.None;

        public List<string> HandlerNames { get; set; }
        [Reactive] public string SelectedHandlerName { get; set; } = null;

        [Reactive] public System.Windows.Media.SolidColorBrush BorderColor { get; set; } = System.Windows.Media.Brushes.Transparent;
        public ReactiveCommand<Unit, Unit> RemoveEntryClick { get; private set; }

        public KeyMappingEntryViewModel(IKeyboardControlledLevelItem handlerProvider, Action<KeyMappingEntryViewModel> removeHandler)
        {
            this.HandlerNames = handlerProvider.GetAllKeyPressHandlerNames().ToList();

            this.RemoveEntryClick = ReactiveCommand.Create(() =>
            {
                removeHandler(this);
            });

            this.WhenAnyValue(x => x.SelectedKey).Subscribe(x =>
            {
                UpdateBorderColor();
            });

            this.WhenAnyValue(x => x.SelectedHandlerName).Subscribe(x =>
            {
                UpdateBorderColor();
            });
        }

        private void UpdateBorderColor()
        {
            if (this.SelectedKey == System.Windows.Input.Key.None && this.SelectedHandlerName == null)
                this.BorderColor = System.Windows.Media.Brushes.Transparent;
            else
                if (GetIsValid())
                this.BorderColor = System.Windows.Media.Brushes.Green;
            else
                this.BorderColor = System.Windows.Media.Brushes.Red;
        }

        private bool GetIsValid()
        {
            return this.SelectedKey != System.Windows.Input.Key.None && this.HandlerNames.IndexOf(SelectedHandlerName) >= 0;
        }

        public KeyboardMappingEntry? GetEntry()
        {
            if (GetIsValid() == false) return null;

            return new KeyboardMappingEntry()
            {
                Key = this.SelectedKey,
                HandlerId = this.HandlerNames.IndexOf(this.SelectedHandlerName)
            };
        }
    }
}
