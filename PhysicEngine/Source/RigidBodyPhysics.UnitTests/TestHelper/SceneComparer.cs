using RigidBodyPhysics.ExportData;

namespace RigidBodyPhysics.UnitTests.TestHelper
{
    internal class SceneComparer
    {
        public static float GetMaxPositionDifference(PhysicSceneExportData scene1, PhysicSceneExportData scene2)
        {
            float max = 0;

            for (int i = 0; i < scene1.Bodies.Length; i++)
            {
                var posDiff = (scene1.Bodies[i].Center - scene2.Bodies[i].Center).Abs();
                max = Math.Max(max, posDiff.X);
                max = Math.Max(max, posDiff.Y);
            }

            return max;
        }
    }
}
