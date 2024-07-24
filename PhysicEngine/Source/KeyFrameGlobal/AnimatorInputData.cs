using GraphicPanelWpf;

namespace KeyFrameGlobal
{
    public class AnimatorInputData
    {
        public IAnimationProperty[] Properties { get; set; } //Alle Propertys von ein Objekt, die animiert werden sollen
        public IAnimationObject AnimationObject { get; set; } //Um das dahinter liegende PhysikModel mit TimeStep zu bewegen
        public IAnimationModelDrawer AnimationModelDrawer { get; set; } //Zum Zeichnen der PhysikScene
    }

    //Getter und Setter für eine Property von ein Objekt, was animiert werden soll
    public interface IAnimationProperty
    {
        object ObjValue { get; set; }
        object Interpolate(object value1, object value2, float time); //time=0 -> Return value1; time=1 -> Return value2
    }

    public interface IFloatAnimationProperty : IAnimationProperty
    {
        float Value { get; set; }
        float MinValue { get; }
        float MaxValue { get; }
    }

    public interface IBoolAnimationProperty : IAnimationProperty
    {
        bool Value { get; set; }
    }

    public interface IAnimationObject : ITimerHandler
    {
        void ResetToInitialState();
    }
}
