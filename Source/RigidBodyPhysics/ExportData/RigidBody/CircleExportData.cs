namespace RigidBodyPhysics.ExportData.RigidBody
{
    public class CircleExportData : PropertysExportData, IExportRigidBody
    {
        public float Radius { get; set; }

        public CircleExportData() { }

        public CircleExportData(CircleExportData copy)
            : base(copy) 
        { 
            Radius = copy.Radius;
        }

        public IExportRigidBody GetCopy()
        {
            return new CircleExportData(this);
        }
    }
}
