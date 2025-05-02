namespace LevelEditorGlobal
{
    public interface IKeyboardControlledLevelItem
    {
        int Id { get; } //ILevelItem.Id
        string[] GetAllKeyPressHandlerNames();
    }
}
