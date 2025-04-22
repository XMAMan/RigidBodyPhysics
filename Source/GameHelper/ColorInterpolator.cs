using GraphicMinimal;

namespace GameHelper
{
    //Erzeugt ein Farbverlauf zwischen vorgegebenen Farbwerten
    public class ColorInterpolator
    {
        private Vector3D[] colors;
        private float sectionLength;
        public ColorInterpolator(Color[] supportColors)
        {
            if (supportColors.Length < 2) throw new ArgumentException("You need at minimum 2 SupportColors");
            this.colors = supportColors.Select(ColorToVector).ToArray();
            this.sectionLength = 1.0f / (supportColors.Length - 1);
        }

        //f = 0..1
        public Color GetColor(float f)
        {
            f = Clamp(f, 0, 1);

            float fDiv = f / this.sectionLength;

            int i1 = (int)fDiv;
            if (i1 > this.colors.Length - 2)
                i1 = this.colors.Length - 2;

            float f1 = fDiv - i1;

            var interpolate = (1 - f1) * this.colors[i1] + f1 * this.colors[i1 + 1];
            return VectorToColor(interpolate);
        }

        private static Vector3D ColorToVector(Color color)
        {
            return new Vector3D(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
        }
        private static Color VectorToColor(Vector3D color)
        {
            color.X = Clamp(color.X, 0, 1);
            color.Y = Clamp(color.Y, 0, 1);
            color.Z = Clamp(color.Z, 0, 1);
            return Color.FromArgb((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255));
        }

        private static float Clamp(float f, float min, float max)
        {
            if (f < min) f = min;
            if (f > max) f = max;
            return f;
        }
    }
}
