namespace WpfControls.Model
{
    public interface IStringSerializable
    {
        string GetExportString();
        void LoadFromExportString(string exportString);
    }
}
