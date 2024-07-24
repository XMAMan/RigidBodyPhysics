using JsonHelper;
using RigidBodyPhysics.ExportData;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.ShapeExporter
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
