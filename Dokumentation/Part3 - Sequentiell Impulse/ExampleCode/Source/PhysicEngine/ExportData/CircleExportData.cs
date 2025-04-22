using GraphicMinimal;

namespace PhysicEngine.ExportData
{
    public class CircleExportData : PropertysExportData, IExportShape
    {
        public Vector2D Center { get; set; }
        public float Radius { get; set; }
        public float AngleInDegree { get; set; }
    }
}
