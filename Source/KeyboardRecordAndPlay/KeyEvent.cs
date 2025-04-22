namespace KeyboardRecordAndPlay
{
    //Objekt, was angelegt wird, wenn ein KeyDown oder KeyUp-Event auftritt
    public class KeyEvent
    {
        public System.Windows.Input.Key Key { get; set; }
        public bool IsKeyDown { get; set; }

        public KeyEvent() { }

        public KeyEvent(System.Windows.Input.Key key, bool isKeyDown)
        {
            this.Key = key;
            this.IsKeyDown = isKeyDown;
        }
    }
}
