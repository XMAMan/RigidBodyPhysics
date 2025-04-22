namespace PhysicEngine.MathHelper
{
    //Quelle: https://github.com/erincatto/box2d-lite/blob/master/include/box2d-lite/MathUtils.h
    public class Matrix2x2
    {
        public readonly Vec2D Col1;
        public readonly Vec2D Col2;
        private Matrix2x2(Vec2D col1, Vec2D col2)
        {
            this.Col1 = col1;
            this.Col2 = col2;
        }

        public static Matrix2x2 FromColumns(Vec2D col1, Vec2D col2)
        {
            return new Matrix2x2 (col1, col2);
        }

        //[a11, a12]
        //[a21, a22]
        public static Matrix2x2 FromScalars(float a11, float a12, float a21, float a22)
        {
            return new Matrix2x2(new Vec2D(a11, a12), new Vec2D(a21, a22));
        }

        public static Matrix2x2 Rotate(float angle)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);

            return new Matrix2x2(new Vec2D(c, s), new Vec2D(-s, c));
        }

        public Matrix2x2 Transpose()
        {
            return new Matrix2x2(new Vec2D(this.Col1.X, this.Col2.X), new Vec2D(this.Col1.Y, this.Col2.Y));
        }

        public Matrix2x2 Invert()
        {
            float a = this.Col1.X, b = this.Col2.X, c = this.Col1.Y, d = this.Col2.Y;
            float det = a * d - b * c;
            if (det == 0) throw new Exception("Determinant cannot be zero");

            det = 1.0f / det;
            return new Matrix2x2(new Vec2D(det * d, - det * c), new Vec2D(-det * b, det * a));
        }

        public float GetDeterminant()
        {
            float a = this.Col1.X, b = this.Col2.X, c = this.Col1.Y, d = this.Col2.Y;
            float det = a * d - b * c;
            return det;
        }

        //Solve A * x = b, where b is a column vector. This is more efficient than computing the inverse in one-shot cases.
        public Vec2D Solve(Vec2D b)
        {
            float det = 1.0f / this.GetDeterminant();
            return new Vec2D(det * (this.Col2.Y * b.X - this.Col1.Y * b.Y),
                             det * (this.Col1.X * b.Y - this.Col2.X * b.X));
        }

        public static Vec2D operator *(Matrix2x2 m, Vec2D v)
        {
            return new Vec2D(m.Col1.X * v.X + m.Col2.X * v.Y, m.Col1.Y * v.X + m.Col2.Y * v.Y);
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
