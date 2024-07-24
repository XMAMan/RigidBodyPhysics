namespace Part4.ViewModel
{
    interface IShapeDataContainer
    {
        string GetShapeData();
        void LoadShapeData(string json);
    }
}
