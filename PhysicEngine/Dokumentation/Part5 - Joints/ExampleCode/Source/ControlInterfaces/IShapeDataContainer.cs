namespace ControlInterfaces
{
    public interface IShapeDataContainer
    {
        string GetShapeData();
        void LoadShapeData(string json);
    }
}
