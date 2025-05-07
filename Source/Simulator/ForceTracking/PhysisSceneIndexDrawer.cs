using RigidBodyPhysics;
using PhysicGlobal;

namespace Simulator.ForceTracking
{
    //Zeichnet von einer PhysicScene alle Body- und Joint-Indizes
    public class PhysisSceneIndexDrawer
    {
        private PhysicScene physicScene;
        private PhysisSceneIndexDrawerSettings forceTrackerDrawingSettings;
        public PhysisSceneIndexDrawer(PhysicScene physicScene, PhysisSceneIndexDrawerSettings forceTrackerDrawingSettings)
        {
            this.physicScene = physicScene;
            this.forceTrackerDrawingSettings = forceTrackerDrawingSettings;
        }

        public void Draw(IDrawingPanel panel)
        {
            float sizeFactor = 1.0f / GetSizeFactorFromCurrentMatrix(panel);

            if (this.forceTrackerDrawingSettings.ShowBodies)
            {
                var bodys = this.physicScene.GetAllBodys();
                for (int i = 0; i < bodys.Length; i++)
                {
                    panel.DrawString(bodys[i].Center, Color.Blue, 40 * sizeFactor, i.ToString());
                }
            }

            if (this.forceTrackerDrawingSettings.ShowJoints)
            {
                var joints = this.physicScene.GetAllJoints();
                for (int i = 0; i < joints.Length; i++)
                {
                    var center = (joints[i].Anchor1 + joints[i].Anchor2) / 2;
                    panel.DrawString(center, Color.Green, 40 * sizeFactor, i.ToString());
                }
            }

            if (this.forceTrackerDrawingSettings.ShowAxialFrictions)
            {
                var axialFrictions = this.physicScene.GetAllAxialFrictions();
                for (int i = 0; i < axialFrictions.Length; i++)
                {
                    panel.DrawString(axialFrictions[i].Anchor, Color.Green, 40 * sizeFactor, i.ToString());
                }
            }                
        }

        private float GetSizeFactorFromCurrentMatrix(IDrawingPanel panel)
        {
            var matrix = panel.GetTransformationMatrix();
            return Matrix4x4.GetSizeFactorFromMatrix(matrix);
        }
    }

    public class PhysisSceneIndexDrawerSettings
    {
        public bool ShowBodies { get; set; } = true;
        public bool ShowJoints { get; set; } = true;
        public bool ShowAxialFrictions { get; set; } = true;
    }
}
