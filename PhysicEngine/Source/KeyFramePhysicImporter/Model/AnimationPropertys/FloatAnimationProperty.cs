using KeyFrameGlobal;

namespace KeyFramePhysicImporter.Model.AnimationPropertys
{
    internal class FloatAnimationProperty : IFloatAnimationProperty
    {
        private Func<float> getter;
        private Action<float> setter;
        public FloatAnimationProperty(float minValue, float maxValue, Func<float> getter, Action<float> setter)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.getter = getter;
            this.setter = setter;
        }

        public object ObjValue
        {
            get
            {
                return this.Value;
            }
            set
            {
                this.Value = (float)value;
            }
        }

        public float Value
        {
            get
            {
                return this.getter();
            }
            set
            {
                this.setter(value);
            }
        }
        public float MinValue { get; }
        public float MaxValue { get; }

        public object Interpolate(object value1, object value2, float time)
        {
            float v1 = (float)value1;
            float v2 = (float)value2;
            return (1 - time) * v1 + time * v2;
        }
    }
}
