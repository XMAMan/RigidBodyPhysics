using GraphicPanelWpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.IO;
using DynamicData;

namespace ElmaControl.Controls.LevelSelect
{
    internal class LevelSelectViewModel : ReactiveObject, IKeyDownUpHandler
    {
        public class Item : ReactiveObject
        {
            [Reactive] public string Text { get; set; }

            public string FileName { get; private set; }

            public Item(string fileName)
            {
                this.FileName = fileName;
                this.Text = Path.GetFileNameWithoutExtension(fileName);
            }
        }

        private string dataFolder;

        public ObservableCollection<Item> Levels { get; set; } = new ObservableCollection<Item>();
        [Reactive] public int SelectedIndex { get; set; } = 0;

        public event Action<string> SelectLevelHandler;
        public event Action GoBackHandler;

        public LevelSelectViewModel(string dataFolder)
        {
            this.dataFolder = dataFolder;
            
        }

        public void Update()
        {
            var files = Directory.GetFiles(dataFolder + "Levels", "*.txt");

            Levels.Clear();
            Levels.AddRange(files
                .Where(x => x.Contains("EmptyLevel.txt") == false)
                .Select(x => new Item(x)));
        }

        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                this.SelectedIndex--;
                if (this.SelectedIndex < 0) this.SelectedIndex = 0;
            }
            if (e.Key == Key.Down)
            {
                this.SelectedIndex++;
                if (this.SelectedIndex >= Levels.Count) this.SelectedIndex = Levels.Count - 1;
            }

            if (e.Key == Key.Enter)
            {
                SelectLevelHandler?.Invoke(Levels[SelectedIndex].FileName);
            }

            if (e.Key == Key.Escape)
            {
                GoBackHandler?.Invoke();
            }
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
        }
    }
}
