using PhysicEngine.MathHelper;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix
{
    //Löst das Gleichungssystem A*x=B
    internal static class ProjectedGaussSeidel
    {
        public static Matrix Solve(Matrix A, Matrix B, Matrix initialX, Matrix minX, Matrix maxX, int iterations)
        {
            if (initialX.IsColumVector() == false) throw new ArgumentException("initialX must be a ColumVector");
            if (A.ColumCount != initialX.RowCount) throw new ArgumentException("A.ColumCount must be equal to initialX.RowCount");
            if (initialX.Size != B.Size) throw new ArgumentException("RowCount from initialX and B must be equal");

            var X = new Matrix(initialX);
            for (int i = 0; i < iterations; i++)
            {
                for (int y=0;y<initialX.RowCount;y++)
                {
                    //Löse X[y] unter Nutzung von Zeile y
                    float sum = 0;
                    for (int j=0;j<initialX.RowCount;j++)
                    {
                        if (j != y)
                            sum += A[j, y] * X[0, j];
                    }
                    X[0, y] = (B[0, y] - sum) / A[y, y];
                    X[0, y] = MathHelp.Clamp(X[0, y], minX[0, y], maxX[0, y]);
                }
            }

            return X;
        }
    }
}
