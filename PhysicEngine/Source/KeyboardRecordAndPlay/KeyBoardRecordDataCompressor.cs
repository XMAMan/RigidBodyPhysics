namespace KeyboardRecordAndPlay
{
    //Wenn man mit den Eventhandlern HandleKeyDown und HandleKeyUp arbeitet, dann ist es wichtig, dass man if (e.IsRepeat) return; benutzt.
    //Wenn man das nicht macht, dann werden bei gedrückter Taste ganz viele KeyDown-Events erzeugt.
    //Wenn ich auf diese Weise mit dem KeyboardRecorder ein KeyBoardRecordData-Objekt erzeugt hat, dann ist das unnötig sehr groß.
    //Diese Klasse hier macht das KeyBoardRecordData-Objekt wieder kleiner, indem doppelte Key-Down/Up-Einträge entfernt werden

    //So muss das e.IsRepeat-Handling aussehen:
    //public void HandleKeyDown(object sender, KeyEventArgs e)
    //{
    //    if (e.IsRepeat) return; //So verhindere ich, dass bei gedrückter Taste der Handler mehrmals aufgerufen wird
    //    this.keyboardRecorder.AddKeyDownEvent(e.Key);
    //}
    //public void HandleKeyUp(object sender, KeyEventArgs e)
    //{
    //    if (e.IsRepeat) return;
    //    this.keyboardRecorder.AddKeyUpEvent(e.Key);
    //}

    //So habe ich die Aufnahmen korrigiert, als ich während der Aufnahme KeyEventArgs.IsRepeat nicht beachtet habe
    //recordData = KeyBoardRecordDataCompressor.Compress(recordData);
    //var data = JsonHelper.Helper.ToJson(recordData);
    //File.WriteAllText(DataFolder + "SkiJumperRecordings\\" + file, data);
    public static class KeyBoardRecordDataCompressor
    {
        public static KeyBoardRecordData Compress(KeyBoardRecordData data)
        {
            //Schritt 1: Gehe durch alle Einträge durch und schaue für jede Taste, dass nur der letzte Eintrag pro Tick dort drin bleibt
            List< KeyEvents > newEvents = new List< KeyEvents >();
            foreach (var tick in data.KeyEvents)
            {
                var lastEventPerKey = tick
                    .Events
                    .GroupBy(x => x.Key)
                    .Select(x => new KeyEvent(x.Key, x.Last().IsKeyDown))
                    .ToList();

                newEvents.Add(new KeyEvents(tick.TimerTick, lastEventPerKey.ToArray()));
            }

            //Schritt 2: Merke dir nur dann ein Key-Down/Up-Event, wenn sich der Zustand einer Taste ändert
            bool[] isDown = new bool[Enum.GetValues(typeof(System.Windows.Input.Key)).Length];
            for (int i = 0; i < isDown.Length; i++) isDown[i] = false;

            foreach (var tick in newEvents)
            {
                var removeList = new List< KeyEvent >();
                foreach (var keyEvent in tick.Events)
                {
                    bool bevore = isDown[(int)keyEvent.Key];
                    bool newState = keyEvent.IsKeyDown;
                    if (bevore == newState)
                    {
                        removeList.Add(keyEvent);
                    }
                    isDown[(int)keyEvent.Key] = newState;
                }
                var oldList = tick.Events.ToList();
                foreach (var del in removeList)
                {
                    oldList.Remove(del);
                }
                tick.Events = oldList.ToArray();
            }

            
            return new KeyBoardRecordData()
            {
                KeyEvents = newEvents.Where(x => x.Events.Any()).ToArray(), //Schritt 3: Entferne all die Ticks, wo keine Events enthalten sind
                TimerTicks = data.TimerTicks
            };
        }
    }
}
