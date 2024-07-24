using GraphicMinimal;

namespace PhysicEngine.MathHelper
{
    //Quelle: https://github.com/erincatto/box2d-lite/blob/master/include/box2d-lite/MathUtils.h
    public class Matrix2x2
    {
        public readonly Vector2D Col1;
        public readonly Vector2D Col2;
        private Matrix2x2(Vector2D col1, Vector2D col2)
        {
            this.Col1 = col1;
            this.Col2 = col2;
        }

        public static Matrix2x2 FromColumns(Vector2D col1, Vector2D col2)
        {
            return new Matrix2x2 (col1, col2);
        }

        //[a11, a12]
        //[a21, a22]
        public static Matrix2x2 FromScalars(float a11, float a12, float a21, float a22)
        {
            return new Matrix2x2(new Vector2D(a11, a12), new Vector2D(a21, a22));
        }

        public static Matrix2x2 Rotate(float angle)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);

            return new Matrix2x2(new Vector2D(c, s), new Vector2D(-s, c));
        }

        public Matrix2x2 Transpose()
        {
            return new Matrix2x2(new Vector2D(this.Col1.X, this.Col2.X), new Vector2D(this.Col1.Y, this.Col2.Y));
        }

        public Matrix2x2 Invert()
        {
            float a = this.Col1.X, b = this.Col2.X, c = this.Col1.Y, d = this.Col2.Y;
            float det = a * d - b * c;
            if (det == 0) throw new Exception("Determinant cannot be zero");

            det = 1.0f / det;
            return new Matrix2x2(new Vector2D(det * d, - det * c), new Vector2D(-det * b, det * a));
        }

        public static Vector2D operator *(Matrix2x2 m, Vector2D v)
        {
            return new Vector2D(m.Col1.X * v.X + m.Col2.X * v.Y, m.Col1.Y * v.X + m.Col2.Y * v.Y);
        }

        public static Matrix2x2 operator +(Matrix2x2 a, Matrix2x2 b)
        {
            return new Matrix2x2(a.Col1 + b.Col1, a.Col2 + b.Col2);
        }

        public static Matrix2x2 operator *(Matrix2x2 a, Matrix2x2 b)
        {
            return new Matrix2x2(a * b.Col1, a * b.Col2);
        }

        public Matrix2x2 Abs()
        {
            return new Matrix2x2(this.Col1.Abs(), this.Col2.Abs());
        }
    }
}
