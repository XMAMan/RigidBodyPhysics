using KeyFrameGlobal;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics;

namespace KeyFramePhysicImporter.Model
{
    internal class PhysicSceneAnimationObject : IAnimationObject
    {
        private PhysicScene physicScene;
        private PhysicSceneExportData intialState;

        public PhysicSceneAnimationObject(PhysicScene physicScene)
        {
            this.physicScene = physicScene;
            this.physicScene.IterationCount = 200;
            this.intialState = physicScene.GetExportData();
        }
        public void HandleTimerTick(float dt)
        {
            this.physicScene.TimeStep(dt);
        }

        public void ResetToInitialState()
        {
            this.physicScene.ResetPosition(this.intialState);
        }
    }
}
