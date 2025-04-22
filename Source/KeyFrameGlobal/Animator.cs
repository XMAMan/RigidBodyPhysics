using static KeyFrameGlobal.AnimationOutputData;

namespace KeyFrameGlobal
{
    //Animiert ein IAnimationProperty[]-Array laut AnimationOutputData. Pro Array darf es mehrere Animator-Objekte geben so lange 
    //sie laut AnimationOutputData.PropertyIsAnimated unterschiedliche Propertys animieren
    public class Animator
    {
        private FrameToTimeConverter frameToTimeConverter;
        private FrameInterpolator frameInterpolator;
        private IAnimationProperty[] propertys;
        private bool[] propertyIsAnimated;
        private AnimationOutputData data;

        public Animator(IAnimationProperty[] propertys, AnimationOutputData data, float framesPerSecond, float initialTime)
        {
            this.propertys = propertys;
            this.frameToTimeConverter = new FrameToTimeConverter((long)(data.DurrationInSeconds * framesPerSecond), data.Type, initialTime);
            this.frameInterpolator = new FrameInterpolator(propertys, data);
            this.propertyIsAnimated = data.PropertyIsAnimated;
            this.data = data;


        }

        public AnimationType Type { get => this.data.Type; }
        public bool PlayBackwards
        {
            set => this.frameToTimeConverter.PlayBackwards = value;
        }
        public bool PlayForward
        {
            set => this.frameToTimeConverter.PlayForward = value;
        }

        public void TimerTick()
        {
            //Gelenk-Sollwerte laut Animationsdatei festlegen
            this.frameToTimeConverter.HandleTimerTick(0); //Erhöhe den internen TimerTick-Zähler
            var frame = frameInterpolator.GetFrame(this.frameToTimeConverter.Time);
            this.propertys.WriteFrameToAnimatedObject(frame, this.propertyIsAnimated); //Schreibe den interpolierten Frame auf das Animation-Anzeigeobjekt
        }

        //Hiermit kann geprüft werden, dass mehrere AnimationOutputData-Objekte nicht die gleiche Property animieren
        public static void CheckThatEachPropertyHasOnlyOneAnimator(int propertyCount, AnimationOutputData[] animations)
        {
            if (animations.Length == 0) throw new ArgumentException("There must be at least one animation");
            for (int i = 0; i < animations.Length; i++)
                if (animations[i].PropertyIsAnimated.Length != propertyCount)
                    throw new ArgumentException("Each Animation must contain the same Property-Count");

            int[] animatedFrom = new int[propertyCount];
            for (int i = 0; i < animatedFrom.Length; i++) animatedFrom[i] = -1;

            for (int i = 0; i < animations.Length; i++)
            {
                for (int j = 0; j < animatedFrom.Length; j++)
                {
                    if (animations[i].PropertyIsAnimated[j])
                    {
                        if (animatedFrom[j] == -1)
                            animatedFrom[j] = i;
                        else
                            throw new ArgumentException($"PropertyIsAnimated[{j}] is animated from Animation {animatedFrom[j]} and {i}. Only one animator per Property is allowed");
                    }
                }
            }
        }
    }
}
