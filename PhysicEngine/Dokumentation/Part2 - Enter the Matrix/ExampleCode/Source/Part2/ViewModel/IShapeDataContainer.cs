namespace Part2.ViewModel
{
    interface IShapeDataContainer
    {
        string GetShapeData();
        void LoadShapeData(string json);
    }
}
