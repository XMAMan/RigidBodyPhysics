namespace WpfControls.Model
{
    public interface IObjectSerializable
    {
        object GetExportObject();
        void LoadFromExportObject(object exportObject);
    }
}
