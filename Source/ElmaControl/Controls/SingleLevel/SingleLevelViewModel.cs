using DynamicData;
using GraphicPanelWpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace ElmaControl.Controls.SingleLevel
{
    internal class SingleLevelViewModel : ReactiveObject, IKeyDownUpHandler
    {
        public enum PreviousState { LevelSelect, BikeIsBroken, LevelFinished, GoBackFromGame, ReplayIsFinished}

        public class Item : ReactiveObject
        {
            [Reactive] public string Text { get; set; } //Anzeigetext
            public string FileName { get; private set; } //Dateiname von den Level, was gespielt werden soll oder von der Keyboard-Record-Datei

            public Item(string text, string fileName)
            {
                this.Text = text;
                this.FileName = fileName;                
            }
        }

        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
        [Reactive] public int SelectedIndex { get; set; } = 0;

        [Reactive]public string SelectedLevelName { get; set; }

        [Reactive] public string FreeText { get; set; } = string.Empty;
        [Reactive] public Brush FreeTextColor { get; set; } = Brushes.Transparent;

        public event Action<string> PlayLevelHandler;           //Parameter: Level-Datei
        public event Action<string, string> ReplayLevelHandler; //Parameter: Level-Datei; Keyboard-Datei
        public event Action GoBackHandler;

        private string dataFolder;
        private string selectedLevelFileName;
        private KeyboardFilesFromLevel keyboardFiles;

        public SingleLevelViewModel(string dataFolder)
        {
            this.dataFolder = dataFolder;
        }


        //Wenn Init aufgerufen wurde, nachdem das Level zuletzt erfolgriech gespielt wurde, dann steht bei keyboardFile die letzte Aufzeichnung
        public SingleLevelViewModel Init(PreviousState previousState, string selectedLevel, string keyboardFile, float timerTickRateInMs)
        {
            this.selectedLevelFileName = selectedLevel;
            this.SelectedLevelName = Path.GetFileNameWithoutExtension(selectedLevel);
            this.keyboardFiles = new KeyboardFilesFromLevel(dataFolder + "Recordings", this.SelectedLevelName, timerTickRateInMs);

            this.Items.Clear();
            this.Items.Add(new Item("Play", selectedLevel));

            if (previousState == PreviousState.LevelFinished || previousState == PreviousState.BikeIsBroken || previousState == PreviousState.ReplayIsFinished)
            {
                this.Items.Add(new Item("Replay last run " + this.keyboardFiles.GetElapsedTime(keyboardFile) , keyboardFile));
            }

            

            switch(previousState)
            {
                case PreviousState.LevelSelect:
                case PreviousState.GoBackFromGame:
                case PreviousState.ReplayIsFinished:
                    this.FreeText = null;
                    break;

                case PreviousState.LevelFinished:
                    this.FreeText = "You finished the Level!";
                    this.FreeTextColor = Brushes.DarkGreen;

                    this.keyboardFiles.TryToAddKeyboardFile(keyboardFile);

                    break;

                case PreviousState.BikeIsBroken:
                    this.FreeText = "Bike is broken!";
                    this.FreeTextColor = Brushes.Red;
                    break;                
            }

            this.Items.AddRange(this.keyboardFiles.GetAllEntrys().Select(x=> new Item(x.Text, x.FileName)));


            this.SelectedIndex = 0;

            return this;
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
                if (this.SelectedIndex >= Items.Count) this.SelectedIndex = Items.Count - 1;
            }

            if (e.Key == Key.Enter)
            {
                if (this.SelectedIndex == 0)
                {
                    PlayLevelHandler?.Invoke(this.selectedLevelFileName);
                }else
                {
                    ReplayLevelHandler?.Invoke(this.selectedLevelFileName, Items[SelectedIndex].FileName);
                }
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
