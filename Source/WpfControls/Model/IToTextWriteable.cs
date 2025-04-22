namespace WpfControls.Model
{
    public interface IToTextWriteable
    {
        void WriteToTextFile(string filePath);
        void LoadFromTextFile(string filePath);
    }
}
