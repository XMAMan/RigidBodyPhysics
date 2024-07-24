using JsonHelper;
using PhysicEngine.ExportData;
using PhysicEngine.RigidBody;

namespace PhysicEngine.UnitTests
{
    internal class ExportHelper
    {
        public static List<IRigidBody> ReadFromFile(string filePath)
        {
            return ExportData.ExportHelper.FromExportData(Helper.FromCompactJson<IExportShape[]>(File.ReadAllText(filePath)));
        }
    }
}
