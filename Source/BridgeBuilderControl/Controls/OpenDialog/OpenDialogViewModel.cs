using DynamicData;
using GraphicPanelWpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Windows.Input;

namespace BridgeBuilderControl.Controls.OpenDialog
{
    internal class OpenDialogViewModel : ReactiveObject, IKeyDownUpHandler
    {
        public class Item : ReactiveObject
        {
            [Reactive] public string Text { get; set; }
            public ReactiveCommand<Unit, Unit> ClickItem { get; private set; }

            public string FileName { get; private set; }

    
            public Item(string fileName, Action<Item> clickHandler)
            {
                this.FileName = fileName;
                this.Text = Path.GetFileNameWithoutExtension(fileName);

                this.ClickItem = ReactiveCommand.Create(() =>
                {
                    clickHandler(this);
                });
            }
        }

        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
        public ReactiveCommand<Unit, Unit> CancelClick { get; private set; }

        public class InputData
        {
            public string InitialDirectory;
            public Action Cancel;
            public Action<string> OpenFileAction; //Parameter: Name von der Datei, die geöffnet werden soll
        }

        private Action cancelAction = () => { };
        private Action<string> openFileAction = (fileName) => { };

        public OpenDialogViewModel()
        {
            this.CancelClick = ReactiveCommand.Create(() =>
            {
                this.cancelAction();
            });
        }

        public OpenDialogViewModel Init(InputData data)
        {
            this.cancelAction = data.Cancel;
            this.openFileAction = data.OpenFileAction;

            var files = Directory.GetFiles(data.InitialDirectory, "*.txt");

            Items.Clear();
            Items.AddRange(files
                .Select(x => new Item(x, ClickItemHandler))
                );

            return this;
        }

        private void ClickItemHandler(Item item)
        {
            this.openFileAction(item.FileName);
        }

        #region IKeyDownUpHandler
        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.CancelClick.Execute().Subscribe();
            }
        }

        public void HandleKeyUp(KeyEventArgs e)
        {

        }
        #endregion
    }
}
