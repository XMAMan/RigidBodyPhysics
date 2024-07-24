using KeyboardRecordAndPlay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ElmaControl.Controls.SingleLevel
{
    //Ermittelt für ein Level alle Keyboard-Files und speichert auch neue Keyboard-Files, wenn sie von der Zeit her besser sind als die alten
    internal class KeyboardFilesFromLevel
    {
        public class Entry
        {
            public string FileName { get; private set; }
            public string Text { get; private set; }

            public Entry(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }
        }

        public Entry[] GetAllEntrys()
        {
            return this.items.Select((x, index) => new Entry(x.FileName, (index + 1) + " " + x.ElapsedTime)).ToArray();
        }

        class KeyboardFile
        {
            public string FileName { get; private set; }
            public string FileNameShort { get; private set; } //{LevelName_{LastNumber}
            public int TimerTicks { get; private set; }
            public string ElapsedTime { get; private set; }

            public byte LastNumber { get; private set; }

            public KeyboardFile(string fileName, float timerTickRateInMs) 
            {
                this.FileName = fileName;
                this.FileNameShort = Path.GetFileNameWithoutExtension(fileName);
                this.TimerTicks = GetTimerTicksFromKeyboardFile(fileName);
                this.ElapsedTime = TimerTickConverter.ToString(this.TimerTicks, timerTickRateInMs);
                this.LastNumber = (byte)(this.FileNameShort.Last() - '0');
            }
        }

        private string dataFolder;
        private string levelName;
        private float timerTickRateInMs;
        private List<KeyboardFile> items = new List<KeyboardFile>();

        public KeyboardFilesFromLevel(string dataFolder, string levelName, float timerTickRateInMs)
        {
            this.dataFolder = dataFolder;
            this.levelName = levelName;
            this.timerTickRateInMs = timerTickRateInMs;

            var files = Directory.GetFiles(dataFolder, "*.txt");

            this.items.AddRange(files
                .Where(x => IsAssociatedRecordFile(levelName, x))
                .Select(x => new KeyboardFile(x, timerTickRateInMs))
                .OrderBy(x => x.TimerTicks)
                );
        }

        //Prüfe, dass die Datei im Format {levelName}_{0..9} ist
        private static bool IsAssociatedRecordFile(string levelName, string fileName)
        {
            string recName = Path.GetFileNameWithoutExtension(fileName);
            return recName.StartsWith(levelName + "_") && (recName.Length == levelName.Length + 2) && IsNumber(recName.Last());
        }

        private static bool IsNumber(char c)
        {
            return (c >= '0' && c <= '9');
        }

        private static int GetTimerTicksFromKeyboardFile(string keyboardFile)
        {
            var recordData = JsonHelper.Helper.CreateFromJson<KeyBoardRecordData>(File.ReadAllText(keyboardFile));
            return recordData.TimerTicks;
        }

        //maxEntrys = Pro Level soll es maximal nur so viele Aufzeichnugen geben
        public void TryToAddKeyboardFile(string lastReplayFile, int maxEntrys = 3)
        {
            int timerTicksFromLastReplay = GetTimerTicksFromKeyboardFile(lastReplayFile);

            //Schritt 1: Lösche all die KeyboardFiles auf der Platte, welche zu schlecht sind
            while (this.items.Count > maxEntrys)
            {
                var lastItem = this.items.Last();
                File.Delete(lastItem.FileName );
                this.items.Remove( lastItem );
            }

            if (this.items.Count < maxEntrys)
            {
                AddNewItem(lastReplayFile);
            }
            else
            {
                var oldItem = this.items[maxEntrys - 1];
                if (oldItem.TimerTicks > timerTicksFromLastReplay)
                {
                    File.Delete(oldItem.FileName);
                    this.items.Remove(oldItem);

                    AddNewItem(lastReplayFile);                    
                }
            }
        }

        private void AddNewItem(string lastReplayFile)
        {
            string newFileName = CreateNewFileName();
            File.Copy(lastReplayFile, newFileName, false );
            var newItem = new KeyboardFile(newFileName, this.timerTickRateInMs);
            this.items.Add(newItem);
            this.items = this.items.OrderBy(x => x.TimerTicks).ToList();
        }

        private string CreateNewFileName()
        {
            List<byte> slots = new List<byte>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            foreach (var entry in this.items)
            {
                slots.Remove(entry.LastNumber);
            }

            if (slots.Any() == false)
                throw new Exception("There are no new slots because numbers 0-9 already in use");

            return dataFolder + "\\" + levelName + "_" + slots.First() + ".txt";
        }

        public string GetElapsedTime(string keyboardFile)
        {
            int ticks = GetTimerTicksFromKeyboardFile(keyboardFile);
            return TimerTickConverter.ToString(ticks, this.timerTickRateInMs);
        }
    }
}
