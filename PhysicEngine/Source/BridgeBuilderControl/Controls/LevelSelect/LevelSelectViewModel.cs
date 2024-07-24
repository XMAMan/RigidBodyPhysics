using DynamicData;
using GraphicPanelWpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace BridgeBuilderControl.Controls.LevelSelect
{
    internal class LevelSelectViewModel : ReactiveObject, IKeyDownUpHandler
    {
        public class Item : ReactiveObject
        {
            [Reactive] public string Text { get; set; }
            public ReactiveCommand<Unit, Unit> ClickItem { get; private set; }

            public string FileName { get; private set; }

            public int NumberSuffix { get; private set; } = -1;//Wenn hinten noch eine Zahl dran hängt dann steht hier welche

            public Item(string fileName, Action<Item> clickHandler)
            {
                this.FileName = fileName;
                this.Text = Path.GetFileNameWithoutExtension(fileName);

                if (Regex.IsMatch(this.Text, "^[^0-9]*([0-9]+)$"))
                {
                    string suffixNumber = Regex.Match(this.Text, "^[^0-9]*([0-9]+)$").Groups[1].Value;
                    this.NumberSuffix = int.Parse(suffixNumber);
                }

                this.ClickItem = ReactiveCommand.Create(() =>
                {
                    clickHandler(this);
                });
            }
        }

        private string dataFolder;
        private Action<string> selectLevelAction;
        public ObservableCollection<Item> Levels { get; set; } = new ObservableCollection<Item>();
        public ReactiveCommand<Unit, Unit> GoBackClick { get; private set; }

        public class InputData
        {
            public Action GoBack;
            public Action<string> SelectLevel;
        }

        public LevelSelectViewModel Init(InputData data)
        {
            this.GoBackClick = ReactiveCommand.Create(() =>
            {
                data.GoBack();
            });
            this.selectLevelAction = data.SelectLevel;
            Update();
            return this;
        }

        public LevelSelectViewModel(string dataFolder)
        {
            this.dataFolder = dataFolder;
        }

        public void Update()
        {
            var files = Directory.GetFiles(dataFolder + "Levels", "*.txt");

            Levels.Clear();
            Levels.AddRange(files
                .Select(x => new Item(x, ClickItemHandler))
                .OrderBy(x => x.NumberSuffix)
                );
        }

        private void ClickItemHandler(Item item)
        {
            this.selectLevelAction(item.FileName);
        }

        #region IKeyDownUpHandler
        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.GoBackClick.Execute().Subscribe();
            }
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
           
        }
        #endregion
    }
}
