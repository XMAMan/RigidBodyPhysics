using System.Runtime.CompilerServices;
using System.Text;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix
{
    public class Matrix
    {
        private float[,] m;

        public int ColumCount { get => m.GetLength(0); }
        public int RowCount { get => m.GetLength(1); }

        public string Size => "[" + ColumCount + ";" + RowCount + "]";

        public float this[int x, int y]
        {
            get => m[x, y];
            set => m[x, y] = value;
        }

        public Matrix(float[,] m)
        {
            this.m = m;
        }

        public Matrix(Matrix copy)
        {
            this.m = new float[copy.ColumCount, copy.RowCount];
            for (int x = 0; x < m.GetLength(0); x++)
                for (int y = 0; y < m.GetLength(1); y++)
                    this.m[x, y] = copy.m[x, y];
        }

        public static Matrix GetColumVector(float[] v)
        {
            float[,] m = new float[1, v.Length];
            for (int i = 0; i < v.Length; i++)
                m[0, i] = v[i];

            return new Matrix(m);
        }

        public static Matrix GetColumVectorWithZeros(int rowCount)
        {
            return GetColumVector(new float[rowCount]);
        }

        public static Matrix GetColumVectorWithInitialValue(int rowCount, float initialValue)
        {
            float[] v = new float[rowCount];
            for (int i = 0; i < rowCount; i++) v[i] = initialValue;
            return GetColumVector(v);
        }

        public static Matrix CreateFromMany(Matrix[] matrizes)
        {
            if (matrizes.Length == 1) return matrizes[0];

            if (matrizes.Select(x => x.ColumCount).Distinct().Count() != 1) throw new ArgumentException("The ColumCount from all matrizes must be equal");

            float[,] m = new float[matrizes[0].ColumCount, matrizes.Sum(x => x.RowCount)];
            int yShift = 0;
            for (int i=0;i<matrizes.Length;i++)
            {                
                for (int x = 0; x < matrizes[i].m.GetLength(0); x++)
                    for (int y = 0; y < matrizes[i].m.GetLength(1); y++)
                        m[x, y + yShift] = matrizes[i].m[x, y];

                yShift += matrizes[i].RowCount;
            }
            
            return new Matrix(m);
        }

        public static Matrix operator +(Matrix a, Matrix b)
        {
            if (a.m.GetLength(0) != b.m.GetLength(0)) throw new ArgumentException("Colum-Count must be equal");
            if (a.m.GetLength(1) != b.m.GetLength(1)) throw new ArgumentException("Row-Count must be equal");

            float[,] m = new float[a.m.GetLength(0), a.m.GetLength(1)];
            for (int x = 0; x < a.m.GetLength(0); x++)
                for (int y = 0; y < a.m.GetLength(1); y++)
                    m[x, y] = a.m[x, y] + b.m[x, y];

            return new Matrix(m);
        }

        public static Matrix operator -(Matrix a, Matrix b)
        {
            if (a.m.GetLength(0) != b.m.GetLength(0)) throw new ArgumentException("Colum-Count must be equal");
            if (a.m.GetLength(1) != b.m.GetLength(1)) throw new ArgumentException("Row-Count must be equal");

            float[,] m = new float[a.m.GetLength(0), a.m.GetLength(1)];
            for (int x = 0; x < a.m.GetLength(0); x++)
                for (int y = 0; y < a.m.GetLength(1); y++)
                    m[x, y] = a.m[x, y] - b.m[x, y];

            return new Matrix(m);
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.m.GetLength(0) != b.m.GetLength(1)) throw new ArgumentException("Columcount from A must be Rowcount from B");

            float[,] m = new float[b.m.GetLength(0), a.m.GetLength(1)];
            for (int x = 0; x < m.GetLength(0); x++)
                for (int y = 0; y < m.GetLength(1); y++)
                {
                    float sum = 0;
                    for (int i = 0; i < a.m.GetLength(0); i++)
                    {
                        sum += a.m[i, y] * b.m[x, i];
                    }
                    m[x, y] = sum;
                }
            return new Matrix(m);
        }

        public static Matrix operator *(Matrix a, float b)
        {
            float[,] m = new float[a.m.GetLength(0), a.m.GetLength(1)];
            for (int x = 0; x < m.GetLength(0); x++)
                for (int y = 0; y < m.GetLength(1); y++)
                    m[x, y] = a.m[x, y] * b;

            return new Matrix(m);
        }

        public static Matrix operator *(float a, Matrix b)
        {
            return b * a;
        }

        public Matrix Transpose()
        {
            float[,] t = new float[this.m.GetLength(1), this.m.GetLength(0)];
            for (int x = 0; x < t.GetLength(0); x++)
                for (int y = 0; y < t.GetLength(1); y++)
                    t[x, y] = this.m[y, x];

            return new Matrix(t);
        }

        public bool IsColumVector()
        {
            return this.ColumCount == 1;
        }

        public bool IsRowVector()
        {
            return this.RowCount == 1;
        }

        public bool IsDiagonal()
        {
            if (this.m.GetLength(0) != this.m.GetLength(1)) return false;

            for (int x = 0; x < this.m.GetLength(0); x++)
                for (int y = 0; y < this.m.GetLength(1); y++)
                {
                    if (x != y)
                        if (m[x, y] != 0) return false;
                        else
                        if (m[x, y] == 0) return false;
                }

            return true;
        }

        public Matrix GetInverted()
        {
            if (IsDiagonal() == false) throw new Exception("At the moment only the inverse from a diagonalmatrix can be calculated");

            float[,] a = new float[m.GetLength(0), m.GetLength(1)];

            for (int x = 0; x < this.m.GetLength(0); x++)
                for (int y = 0; y < this.m.GetLength(1); y++)
                    a[x, y] = 1.0f / this.m[x, y];

            return new Matrix(a);
        }

        public float[] GetColum(int colIndex)
        {
            float[] c = new float[this.RowCount];
            for (int i = 0; i < this.RowCount; i++)
                c[i] = this.m[colIndex, i];

            return c;
        }

        public float GetSqrtLength()
        {
            if (this.IsColumVector() == false) throw new Exception("Matrix must be a ColumVector");

            double sum = 0;
            for (int i=0;i< this.RowCount; i++)
                sum += this.m[0, i] * this.m[0, i];

            return (float)Math.Sqrt(sum);
        }

        public override string ToString()
        {
            int maxLength = -1;
            string[,] fields = new string[this.ColumCount, this.RowCount];
            for (int x = 0; x < this.m.GetLength(0); x++)
                for (int y = 0; y < this.m.GetLength(1); y++)
                {
                    //fields[x, y] = String.Format("{0:+0.00;-0.00; 0.00}", this.m[x, y]);
                    fields[x, y] = String.Format("{0:+0.00;-0.00; 0.00}".Replace("00", "000000"), this.m[x, y]);
                    if (fields[x, y].Length > maxLength) maxLength = fields[x, y].Length;
                }

            StringBuilder str = new StringBuilder();
            for (int y = 0; y < this.RowCount; y++)
            {
                str.Append("|");
                for (int x = 0; x < this.ColumCount; x++)
                {
                    int p = x < this.m.GetLength(0) - 1 ? 1 : 0;
                    str.Append(fields[x, y] + new string(' ', maxLength + p - fields[x, y].Length));
                }
                str.Append("|" + System.Environment.NewLine);
            }

            return str.ToString();
        }
    }
}
