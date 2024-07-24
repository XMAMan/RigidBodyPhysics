namespace Part1.ViewModel
{
    interface IShapeDataContainer
    {
        string GetShapeData();
        void LoadShapeData(string json);
    }
}
