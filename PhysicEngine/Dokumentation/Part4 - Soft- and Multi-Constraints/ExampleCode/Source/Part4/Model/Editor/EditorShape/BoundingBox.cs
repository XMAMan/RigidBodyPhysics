using GraphicMinimal;

namespace Part4.Model.Editor.EditorShape
{
    internal class BoundingBox
    {
        public Vector2D Min { get; private set; }
        public Vector2D Max { get; private set; }

        public BoundingBox(Vector2D min, Vector2D max)
        {
            Min = min;
            Max = max;
        }
    }
}
