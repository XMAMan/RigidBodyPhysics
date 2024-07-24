using GraphicPanelWpf;
using KeyFrameGlobal;
using RigidBodyPhysics.ExportData;

namespace KeyFramePhysicImporter.Model
{
    //Menge von Gelenken und Thrusters, die per Animation gesteuert werden
    public class AnimatedPhysicObjects : ITimerHandler
    {
        public PhysicScenePublicData PhysicObjects { get; private set; }
        public Animator[] Animators { get; private set; }
        public AnimationOutputData[] AnimationData { get; private set; }

        public AnimatedPhysicObjects(PhysicScenePublicData physicObjects, AnimationOutputData[] animations, float timerIntercallInMilliseconds)
        {
            var propertys = PhysicSceneAnimationPropertyConverter.Convert(physicObjects); //Hiermit wird die PhysicScene gesteuert
            Animator.CheckThatEachPropertyHasOnlyOneAnimator(propertys.Length, animations);
            float framesPerSecond = 1000 / timerIntercallInMilliseconds; //Wenn ich den Timer mit diesen Intervall nutze, dann ergeben sich daraus so viele TimerTicks pro Sekunde

            this.PhysicObjects = physicObjects;
            this.Animators = animations.Select(x => new Animator(propertys, x, framesPerSecond, x.StartTime)).ToArray();
            this.AnimationData = animations;
        }

        public void HandleTimerTick(float dt)
        {
            foreach (var animator in Animators)
            {
                animator.TimerTick();
            }
        }
    }
}
