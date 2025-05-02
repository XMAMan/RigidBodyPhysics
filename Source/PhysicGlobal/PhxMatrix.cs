namespace PhysicGlobal
{
    //4*4-Matrix, welche für die Transformation von 2D-Objekten genutzt wird
    public class PhxMatrix
    {
        public float[] Values { get; private set; }

        public PhxMatrix(float[] values)
        {
            this.Values = values;
        }

        public static PhxMatrix Ident()
        {
            return new PhxMatrix(new float[] {1, 0, 0, 0,
                                              0, 1, 0, 0,
                                              0, 0, 1, 0,
                                              0, 0, 0, 1});
        }

        public static PhxMatrix Translate(float x, float y, float z)
        {
            return new PhxMatrix(new float[] {1,    0,    0,    0,
                                              0,    1,    0,    0,
                                              0,    0,    1,    0,
                                              x,    y,    z,    1});
        }

        public static PhxMatrix Scale(float x, float y, float z)
        {
            return new PhxMatrix(new float[] {x, 0, 0, 0,
                                              0, y, 0, 0,
                                              0, 0, z, 0,
                                              0, 0, 0, 1});
        }

        //Quelle: http://www.gamedev.net/topic/600537-instead-of-glrotatef-build-a-matrix/
        //[x | y | z] - Drehachse
        public static PhxMatrix Rotate(float angle, float x, float y, float z)
        {
            float c = (float)Math.Cos(angle * Math.PI / 180);
            float s = (float)Math.Sin(angle * Math.PI / 180);

            return new PhxMatrix(
                new float[] {x * x * (1-c)+c,       y * x * (1-c)+z*s,  x*z*(1-c)-y*s,  0,
                             x*y*(1-c)-z*s,         y*y*(1-c)+c,        y*z*(1-c)+x*s,  0,
                             x*z*(1-c)+y*s,         y*z*(1-c)-x*s,      z*z*(1-c)+c,    0,
                             0,                     0,                  0,              1});
        }

        public static PhxMatrix operator *(PhxMatrix m1, PhxMatrix m2)
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

            return new PhxMatrix(R);
        }

        public static Vec2D MultPosition(PhxMatrix matrix, Vec2D position)
        {
            var m = matrix.Values;
            Vec2D res = new Vec2D(m[0] * position.X + m[4] * position.Y + m[12],
                                  m[1] * position.X + m[5] * position.Y + m[13]);
            return res;
        }

        public static float GetSizeFactorFromMatrix(PhxMatrix matrix)
        {
            var p1 = PhxMatrix.MultPosition(matrix, new Vec2D(0, 0));
            var p2 = PhxMatrix.MultPosition(matrix, new Vec2D(1, 0));
            return (p2 - p1).Length();
        }

        public static float GetAngleInDegreeFromMatrix(PhxMatrix matrix)
        {
            var p1 = PhxMatrix.MultPosition(matrix, new Vec2D(0, 0));
            var p2 = PhxMatrix.MultPosition(matrix, new Vec2D(1, 0));
            return Vec2D.Angle360(new Vec2D(1, 0), p2 - p1);
        }
    }
}
