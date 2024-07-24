using GraphicMinimal;

namespace PhysicEngine.ExportData.RigidBody
{
    public class CircleExportData : PropertysExportData, IExportRigidBody
    {
        public Vector2D Center { get; set; }
        public float Radius { get; set; }
        public float AngleInDegree { get; set; }
    }
}
