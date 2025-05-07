namespace PhysicGlobal
{
    //4*4-Matrix, welche für die Transformation von 2D-Objekten genutzt wird
    public class Matrix4x4
    {
        public float[] Values { get; private set; }

        public Matrix4x4(float[] values)
        {
            this.Values = values;
        }

        public static Matrix4x4 Ident()
        {
            return new Matrix4x4(new float[] {1, 0, 0, 0,
                                              0, 1, 0, 0,
                                              0, 0, 1, 0,
                                              0, 0, 0, 1});
        }

        public static Matrix4x4 Translate(float x, float y, float z)
        {
            return new Matrix4x4(new float[] {1,    0,    0,    0,
                                              0,    1,    0,    0,
                                              0,    0,    1,    0,
                                              x,    y,    z,    1});
        }

        public static Matrix4x4 Scale(float x, float y, float z)
        {
            return new Matrix4x4(new float[] {x, 0, 0, 0,
                                              0, y, 0, 0,
                                              0, 0, z, 0,
                                              0, 0, 0, 1});
        }

        //Quelle: http://www.gamedev.net/topic/600537-instead-of-glrotatef-build-a-matrix/
        //[x | y | z] - Drehachse
        public static Matrix4x4 Rotate(float angle, float x, float y, float z)
        {
            float c = (float)Math.Cos(angle * Math.PI / 180);
            float s = (float)Math.Sin(angle * Math.PI / 180);

            return new Matrix4x4(
                new float[] {x * x * (1-c)+c,       y * x * (1-c)+z*s,  x*z*(1-c)-y*s,  0,
                             x*y*(1-c)-z*s,         y*y*(1-c)+c,        y*z*(1-c)+x*s,  0,
                             x*z*(1-c)+y*s,         y*z*(1-c)-x*s,      z*z*(1-c)+c,    0,
                             0,                     0,                  0,              1});
        }

        public static Matrix4x4 operator *(Matrix4x4 m1, Matrix4x4 m2)
        {
            float[] P1 = m1.Values;
            float[] P2 = m2.Values;

            if (P2.Length != 16 || P1.Length != 16) return null;
            float[] R = new float[16];

            R[0] = P2[0] * P1[0] + P2[4] * P1[1] + P2[8] * P1[2] + P2[12] * P1[3]; //1. Spaltenvektor
            R[1] = P2[1] * P1[0] + P2[5] * P1[1] + P2[9] * P1[2] + P2[13] * P1[3];
            R[2] = P2[2] * P1[0] + P2[6] * P1[1] + P2[10] * P1[2] + P2[14] * P1[3];
            R[3] = P2[3] * P1[0] + P2[7] * P1[1] + P2[11] * P1[2] + P2[15] * P1[3];

            R[4] = P2[0] * P1[4] + P2[4] * P1[5] + P2[8] * P1[6] + P2[12] * P1[7]; //2. Spaltenvektor
            R[5] = P2[1] * P1[4] + P2[5] * P1[5] + P2[9] * P1[6] + P2[13] * P1[7];
            R[6] = P2[2] * P1[4] + P2[6] * P1[5] + P2[10] * P1[6] + P2[14] * P1[7];
            R[7] = P2[3] * P1[4] + P2[7] * P1[5] + P2[11] * P1[6] + P2[15] * P1[7];

            R[8] = P2[0] * P1[8] + P2[4] * P1[9] + P2[8] * P1[10] + P2[12] * P1[11]; //3. Spaltenvektor
            R[9] = P2[1] * P1[8] + P2[5] * P1[9] + P2[9] * P1[10] + P2[13] * P1[11];
            R[10] = P2[2] * P1[8] + P2[6] * P1[9] + P2[10] * P1[10] + P2[14] * P1[11];
            R[11] = P2[3] * P1[8] + P2[7] * P1[9] + P2[11] * P1[10] + P2[15] * P1[11];

            R[12] = P2[0] * P1[12] + P2[4] * P1[13] + P2[8] * P1[14] + P2[12] * P1[15];//4. Spaltenvektor
            R[13] = P2[1] * P1[12] + P2[5] * P1[13] + P2[9] * P1[14] + P2[13] * P1[15];
            R[14] = P2[2] * P1[12] + P2[6] * P1[13] + P2[10] * P1[14] + P2[14] * P1[15];
            R[15] = P2[3] * P1[12] + P2[7] * P1[13] + P2[11] * P1[14] + P2[15] * P1[15];

            return new Matrix4x4(R);
        }

        public static Vec2D MultPosition(Matrix4x4 matrix, Vec2D position)
        {
            var m = matrix.Values;
            Vec2D res = new Vec2D(m[0] * position.X + m[4] * position.Y + m[12],
                                  m[1] * position.X + m[5] * position.Y + m[13]);
            return res;
        }

        public static float GetSizeFactorFromMatrix(Matrix4x4 matrix)
        {
            var p1 = Matrix4x4.MultPosition(matrix, new Vec2D(0, 0));
            var p2 = Matrix4x4.MultPosition(matrix, new Vec2D(1, 0));
            return (p2 - p1).Length();
        }

        public static float GetAngleInDegreeFromMatrix(Matrix4x4 matrix)
        {
            var p1 = Matrix4x4.MultPosition(matrix, new Vec2D(0, 0));
            var p2 = Matrix4x4.MultPosition(matrix, new Vec2D(1, 0));
            return Vec2D.Angle360(new Vec2D(1, 0), p2 - p1);
        }

        //http://www.cg.info.hiroshima-cu.ac.jp/~miyazaki/knowledge/teche23.html
        public static Matrix4x4 Invert(Matrix4x4 matrix4x4)
        {
            float[] m = matrix4x4.Values;

            float determinant = m[0] * m[5] * m[10] * m[15] + m[0] * m[6] * m[11] * m[13] + m[0] * m[7] * m[9] * m[14]
                                + m[1] * m[4] * m[11] * m[14] + m[1] * m[6] * m[8] * m[15] + m[1] * m[7] * m[10] * m[12]
                                + m[2] * m[4] * m[9] * m[15] + m[2] * m[5] * m[11] * m[12] + m[2] * m[7] * m[8] * m[13]
                                + m[3] * m[4] * m[10] * m[13] + m[3] * m[5] * m[8] * m[14] + m[3] * m[6] * m[9] * m[12]
                                - m[0] * m[5] * m[11] * m[14] - m[0] * m[6] * m[9] * m[15] - m[0] * m[7] * m[10] * m[13]
                                - m[1] * m[4] * m[10] * m[15] - m[1] * m[6] * m[11] * m[12] - m[1] * m[7] * m[8] * m[14]
                                - m[2] * m[4] * m[11] * m[13] - m[2] * m[5] * m[8] * m[15] - m[2] * m[7] * m[9] * m[12]
                                - m[3] * m[4] * m[9] * m[14] - m[3] * m[5] * m[10] * m[12] - m[3] * m[6] * m[8] * m[13];
            if (determinant == 0) throw new Exception("Can not create inverse because determinant is zero");

            float b11 = m[5] * m[10] * m[15] + m[6] * m[11] * m[13] + m[7] * m[9] * m[14] - m[5] * m[11] * m[14] - m[6] * m[9] * m[15] - m[7] * m[10] * m[13];
            float b12 = m[1] * m[11] * m[14] + m[2] * m[9] * m[15] + m[3] * m[10] * m[13] - m[1] * m[10] * m[15] - m[2] * m[11] * m[13] - m[3] * m[9] * m[14];
            float b13 = m[1] * m[6] * m[15] + m[2] * m[7] * m[13] + m[3] * m[5] * m[14] - m[1] * m[7] * m[14] - m[2] * m[5] * m[15] - m[3] * m[6] * m[13];
            float b14 = m[1] * m[7] * m[10] + m[2] * m[5] * m[11] + m[3] * m[6] * m[9] - m[1] * m[6] * m[11] - m[2] * m[7] * m[9] - m[3] * m[5] * m[10];
            float b21 = m[4] * m[11] * m[14] + m[6] * m[8] * m[15] + m[7] * m[10] * m[12] - m[4] * m[10] * m[15] - m[6] * m[11] * m[12] - m[7] * m[8] * m[14];
            float b22 = m[0] * m[10] * m[15] + m[2] * m[11] * m[12] + m[3] * m[8] * m[14] - m[0] * m[11] * m[14] - m[2] * m[8] * m[15] - m[3] * m[10] * m[12];
            float b23 = m[0] * m[7] * m[14] + m[2] * m[4] * m[15] + m[3] * m[6] * m[12] - m[0] * m[6] * m[15] - m[2] * m[7] * m[12] - m[3] * m[4] * m[14];
            float b24 = m[0] * m[6] * m[11] + m[2] * m[7] * m[8] + m[3] * m[4] * m[10] - m[0] * m[7] * m[10] - m[2] * m[4] * m[11] - m[3] * m[6] * m[8];
            float b31 = m[4] * m[9] * m[15] + m[5] * m[11] * m[12] + m[7] * m[8] * m[13] - m[4] * m[11] * m[13] - m[5] * m[8] * m[15] - m[7] * m[9] * m[12];
            float b32 = m[0] * m[11] * m[13] + m[1] * m[8] * m[15] + m[3] * m[9] * m[12] - m[0] * m[9] * m[15] - m[1] * m[11] * m[12] - m[3] * m[8] * m[13];
            float b33 = m[0] * m[5] * m[15] + m[1] * m[7] * m[12] + m[3] * m[4] * m[13] - m[0] * m[7] * m[13] - m[1] * m[4] * m[15] - m[3] * m[5] * m[12];
            float b34 = m[0] * m[7] * m[9] + m[1] * m[4] * m[11] + m[3] * m[5] * m[8] - m[0] * m[5] * m[11] - m[1] * m[7] * m[8] - m[3] * m[4] * m[9];
            float b41 = m[4] * m[10] * m[13] + m[5] * m[8] * m[14] + m[6] * m[9] * m[12] - m[4] * m[9] * m[14] - m[5] * m[10] * m[12] - m[6] * m[8] * m[13];
            float b42 = m[0] * m[9] * m[14] + m[1] * m[10] * m[12] + m[2] * m[8] * m[13] - m[0] * m[10] * m[13] - m[1] * m[8] * m[14] - m[2] * m[9] * m[12];
            float b43 = m[0] * m[6] * m[13] + m[1] * m[4] * m[14] + m[2] * m[5] * m[12] - m[0] * m[5] * m[14] - m[1] * m[6] * m[12] - m[2] * m[4] * m[13];
            float b44 = m[0] * m[5] * m[10] + m[1] * m[6] * m[8] + m[2] * m[4] * m[9] - m[0] * m[6] * m[9] - m[1] * m[4] * m[10] - m[2] * m[5] * m[8];

            float[] inverse = {b11, b12, b13, b14,
                               b21, b22, b23, b24,
                               b31, b32, b33, b34,
                               b41, b42, b43, b44};

            float invDet = 1.0f / determinant;

            for (int i = 0; i < inverse.Length; i++) inverse[i] *= invDet;

            return new Matrix4x4(inverse);
        }
    }
}
