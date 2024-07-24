using PhysicEngine.RigidBody;

namespace PhysicEngine.ExportData
{
    public static class ExportHelper
    {
        public static IExportShape[] ToExportData(List<IRigidBody> exportable)
        {
            return exportable.Select(x => x.GetExportData()).ToArray();
        }

        public static List<IRigidBody> FromExportData(IExportShape[] exportShapes)
        {
            List<IRigidBody> result = new List<IRigidBody>();

            foreach (var shape in exportShapes)
            {
                if (shape is RectangleExportData)
                    result.Add(new RigidRectangle((shape as RectangleExportData)));

                if (shape is CircleExportData)
                    result.Add(new RigidCircle((shape as CircleExportData)));
            }

            return result;
        }
    }
}
