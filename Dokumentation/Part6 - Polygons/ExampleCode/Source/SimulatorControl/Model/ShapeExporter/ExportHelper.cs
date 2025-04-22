using JsonHelper;
using PhysicEngine.ExportData;

namespace SimulatorControl.Model.ShapeExporter
{
    internal static class ExportHelper
    {
        #region Simulator
        public static string ToJson(PhysicSceneExportData physicScene)
        {
            return Helper.ToCompactJson(physicScene);
        }

        #endregion
    }
}
