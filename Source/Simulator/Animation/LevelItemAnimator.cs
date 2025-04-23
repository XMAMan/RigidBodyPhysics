using GraphicPanelWpf;
using KeyFrameGlobal;
using KeyFramePhysicImporter.Model;
using RigidBodyPhysics.ExportData;

namespace Simulator.Animation
{
    //Speichert für jedes LevelItem ein AnimatedPhysicObjects
    public class LevelItemAnimator : ITimerHandler
    {
        private List<AnimatedPhysicObjects> levelItems = new List<AnimatedPhysicObjects>();
        private float timerIntervalInMilliseconds;

        public LevelItemAnimator(float timerIntervalInMilliseconds)
        {
            this.timerIntervalInMilliseconds = timerIntervalInMilliseconds;
        }

        public void AddLevelItem(PhysicScenePublicData physicObjects, AnimationOutputData[] animations)
        {
            levelItems.Add(new AnimatedPhysicObjects(physicObjects, animations, this.timerIntervalInMilliseconds));
        }

        public Animator[] GetAnimationRuntimDataFromLevelItem(PhysicScenePublicData physicObjects)
        {
            return this.levelItems.First(x => x.PhysicObjects == physicObjects).Animators;
        }

        public AnimationOutputData[] GetAnimationExportDataFromLevelItem(PhysicScenePublicData physicObjects)
        {
            var exportData = this.levelItems
                .FirstOrDefault(x => x.PhysicObjects == physicObjects)?.AnimationData;       
            
            if (exportData == null) return new AnimationOutputData[0];

            return exportData;
        }

        public void RemoveLevelItem(PhysicScenePublicData physicObjects)
        {
            var del = this.levelItems.FirstOrDefault(x => x.PhysicObjects == physicObjects);
            if (del != null)
            {
                this.levelItems.Remove(del);
            }            
        }

        public void HandleTimerTick(float dt)
        {
            foreach (var item in levelItems)
            {
                item.HandleTimerTick(dt);
            }
        }
    }
}
