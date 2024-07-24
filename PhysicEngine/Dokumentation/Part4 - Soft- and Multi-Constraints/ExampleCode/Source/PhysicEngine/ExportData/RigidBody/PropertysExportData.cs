using GraphicMinimal;

namespace PhysicEngine.ExportData.RigidBody
{
    public class PropertysExportData
    {
        public Vector2D Velocity { get; set; } = new Vector2D(0, 0);
        public float AngularVelocity { get; set; } = 0;
        public MassData MassData { get; set; } = null;
        public float Friction { get; set; } = 1;
        public float Restituion { get; set; } = 1;
    }
}
