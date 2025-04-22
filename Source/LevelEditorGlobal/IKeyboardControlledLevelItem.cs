namespace LevelEditorGlobal
{
    public interface IKeyboardControlledLevelItem
    {
        int Id { get; } //ILevelItem.Id
        string[] GetAllKeyPressHandlerNames();
    }

    //All die Keyboardmappings für ein einzelnes LevelItem (Wird vom internen Simulator benutzt)
    public class KeyboardMappingTable
    {
        public int LevelItemId { get; set; } //Von diesen LevelItem wurde eine Mapping-Tabelle erstellt
        public KeyboardMappingEntry[] Entries { get; set; } //Entries.HandlerId = Der wie vielte Handler von den jeweiligen LevelItem(PhysicScene) ist das?

        public KeyboardMappingTable() { } //Für den Serialisierer

        public KeyboardMappingTable(int levelItemId, KeyboardMappingEntry[] entries)
        {
            this.LevelItemId = levelItemId;
            this.Entries = entries;
        }
    }

    public class KeyboardMappingEntry
    {
        public System.Windows.Input.Key Key;
        public int HandlerId; //Index aus PhysicSceneKeyPressHandlerProvider.GetHandler(physicScene, animators)

        public KeyboardMappingEntry() { }
        public KeyboardMappingEntry(KeyboardMappingEntry copy)
        {
            Key = copy.Key;
            HandlerId = copy.HandlerId;
        }
    }
}
