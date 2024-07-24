using GraphicMinimal;

namespace PhysicEngine.ExportData.RigidBody
{
    public class RectangleExportData : PropertysExportData, IExportRigidBody
    {
        public Vector2D Center { get; set; }
        public Vector2D Size { get; set; }
        public float AngleInDegree { get; set; }
    }
}
