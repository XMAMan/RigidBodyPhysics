namespace Part3.ViewModel
{
    interface IShapeDataContainer
    {
        string GetShapeData();
        void LoadShapeData(string json);
    }
}
