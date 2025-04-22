using KeyFrameGlobal;

namespace KeyFramePhysicImporter.Model.AnimationPropertys
{
    internal class BoolAnimationProperty : IBoolAnimationProperty
    {
        private Func<bool> getter;
        private Action<bool> setter;
        public BoolAnimationProperty(Func<bool> getter, Action<bool> setter)
        {
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
                this.Value = (bool)value;
            }
        }
        public bool Value
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

        public object Interpolate(object value1, object value2, float time)
        {
            return value1;
        }
    }
}
