using GraphicMinimal;

namespace PhysicEngine.ExportData
{
    public class RectangleExportData : PropertysExportData, IExportShape
    {
        public Vector2D Center { get; set; }
        public Vector2D Size { get; set; }
        public float AngleInDegree { get; set; }
    }
}
