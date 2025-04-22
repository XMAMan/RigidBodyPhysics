using LevelEditorGlobal;
using RigidBodyPhysics;

namespace Simulator
{
    //Konvertiert ein PhysikLevelItemExportData-Array in ein PhysicScene-Objekt und ein RuntimeLevelItem-Array
    internal static class ExportToRuntimeConverter
    {
        internal static void Convert(PhysikLevelItemExportData[] exportItems, bool[,] collisionMatrix, out PhysicScene physicScene, out RuntimeLevelItem[] runtimeItems)
        {
            List<RuntimeLevelItem> returnList = new List<RuntimeLevelItem>();

            physicScene = new PhysicScene(new RigidBodyPhysics.ExportData.PhysicSceneExportData()
            {
                CollisionMatrix = collisionMatrix,
            });

            foreach (var item in exportItems)
            {
                var physicData = physicScene.AddPhysicScene(item.PhysicSceneData);

                returnList.Add(new RuntimeLevelItem(item.LevelItemId, physicData));
            }

            runtimeItems = returnList.ToArray();
        }
    }
}
