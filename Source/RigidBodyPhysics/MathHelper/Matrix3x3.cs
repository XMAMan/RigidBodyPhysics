namespace RigidBodyPhysics.MathHelper
{
    internal class Matrix3x3
    {
        public float[] Values { get; private set; }

        public Matrix3x3(float[] values)
        {
            this.Values = values;
        }


        //http://www.cg.info.hiroshima-cu.ac.jp/~miyazaki/knowledge/teche23.html
        public Matrix3x3 Invert()
        {
            float[] m = this.Values;

            float determinant = m[0] * m[4] * m[8] + m[3] * m[7] * m[2] + m[6] * m[1] * m[5] - m[0] * m[7] * m[5] - m[6] * m[4] * m[2] - m[3] * m[1] * m[8];

            if (determinant == 0) throw new Exception("Can not create inverse because determinant is zero");

            float[] inverse = new float[]
            {
                m[4] * m[8]-m[5] * m[7],    m[2]*m[7]-m[1]*m[8],    m[1]*m[5]-m[2]*m[4],
                m[5]*m[6]-m[3]*m[8],        m[0]*m[8]-m[2]*m[6],    m[2]*m[3]-m[0]*m[5],
                m[3]*m[7]-m[4]*m[6],        m[1]*m[6]-m[0]*m[7],    m[0]*m[4]-m[1]*m[3]
            };

            float invDet = 1.0f / determinant;

            for (int i = 0; i < inverse.Length; i++) inverse[i] *= invDet;

            return new Matrix3x3(inverse);
        }

        //Return matrix*v
        public static Vec3D operator *(Matrix3x3 matrix, Vec3D v)
        {
            float[] m = matrix.Values;

            float x = m[0] * v.X + m[3] * v.Y + m[6] * v.Z;
            float y = m[1] * v.X + m[4] * v.Y + m[7] * v.Z;
            float z = m[2] * v.X + m[5] * v.Y + m[8] * v.Z;

            return new Vec3D(x, y, z);
        }
    }
}
