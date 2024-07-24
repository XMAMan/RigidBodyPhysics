using PhysicEngine.CollisionResolution.EnterTheMatrix.Constraints;
using PhysicEngine.RigidBody;
using System.Text;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix
{
    //Enthält all die Matrizen für folgende Gleichung: J * mInverse * jTransposed * lambda = inv_dt * bias - J * (inv_dt * V + mInverse*fExt)
    internal class EquationOfMotionData
    {
        private Matrix V;
        private Matrix fExt;
        private Matrix mInverse;
        private Matrix bias;
        private Matrix J;
        private Matrix jTransposed;
        private Matrix initialLambda;
        private Matrix minLambda;
        private Matrix maxLambda;
        private Matrix lambda;
        private Matrix vNew;
        private Matrix A;
        private Matrix B;

        public EquationOfMotionData(List<IRigidBody> bodies, CollisionPointWithLambda[] collisions, float dt, SolverSettings settings)
        {
            float inv_dt = dt > 0.0f ? 1.0f / dt : 0.0f;

            //Schritt 1: Gleichungssystem aufstellen, womit ich Lambda lösen kann
            var constraints = new AllConstraints(new ConstraintConstructorData()
            {
                Bodies = bodies,
                Collisions = collisions,
                Dt = dt,
                Settings = settings
            });
            this.J = constraints.GetJacobian();
            this.mInverse = BodiesToMatrix.GetInverseMassMatrix(bodies);
            this.jTransposed = J.Transpose();
            this.bias = constraints.GetBias();
            this.V = BodiesToMatrix.GetV(bodies);
            this.fExt = BodiesToMatrix.GetFext(bodies);

            //A*Lambda=B -> Gesucht: Lambda
            this.A = J * mInverse * jTransposed;
            this.B = inv_dt * bias - J * (inv_dt * V + mInverse * fExt);

            //Schritt 2: Lambda per PGS ermitteln            
            this.minLambda = constraints.GetMinLambda();
            this.maxLambda = constraints.GetMaxLambda();
            this.initialLambda = settings.DoWarmStart ? constraints.GetInitialLambda() : Matrix.GetColumVectorWithZeros(this.minLambda.RowCount);
			this.lambda = ProjectedGaussSeidel.Solve(A, B, initialLambda, minLambda, maxLambda, settings.IterationCount);
            constraints.SaveLambdaInCollisionPoints(this.lambda);

            //string pgsSteps = GetProjectedGaussSeidelSteps(A, B, initialLambda, minLambda, maxLambda, 10);

            //Schritt 3: Mit Lambda ein V ermitteln, was alle Constraints erfüllt
            this.vNew = mInverse * dt * (jTransposed * lambda + fExt) + V;

            //var JV = (J * V).ToString(); //Mit dieser Relativgeschwindigkeit bewegen sich die Kontaktpunkte vor der Berechung
            //var JVnew = (J * vNew).ToString(); //Mit dieser Relativgeschwindigkeit bewegen sich die Kontaktpunkte nach der Berechung (Ziel: So wie der Bias-Wert sein)
            //var jTV = (jTransposed * lambda).ToString(); //Das ist die Constraintkraft

            //Linke Seite der Bewegungsgleichung
            //var jString = J.ToString();
            //var mInv = mInverse.ToString();
            //var jMInverse = (J * mInverse).ToString();
            //var jMInversejTransposed = (J * mInverse * jTransposed).ToString();

            //Rechte Seite der Bewegungsgleichung
            //var dtBias = (inv_dt * bias).ToString();
            //var dtV = (inv_dt * V).ToString();
            //var mInverseFExt = (mInverse * fExt).ToString();
            //var dtVmInverseFExt = (inv_dt * V + mInverse * fExt).ToString();
            //var JdtVmInverseFExt = (J * (inv_dt * V + mInverse * fExt)).ToString();
            //var bString = B.ToString();

            //DeltaV-Gleichung
            //var jTLambda = (jTransposed * lambda).ToString(); //Das ist die Constraintkraft
            //var forceAll = (jTransposed * lambda + fExt).ToString();
            //var forceAllDt = (dt * (jTransposed * lambda + fExt)).ToString();
            //var deltaV = (mInverse * dt * (jTransposed * lambda + fExt)).ToString();
        }

        //Schritt 4: Die Geschwindigkeitswerte aller Körper korrigieren
        public void SetVelocityValues(List<IRigidBody> bodies)
        {
            float[] vNew = this.vNew.GetColum(0);

            for (int i = 0; i < bodies.Count; i++)
            {
                bodies[i].Velocity.X = vNew[i * 3 + 0];
                bodies[i].Velocity.Y = vNew[i * 3 + 1];
                bodies[i].AngularVelocity = vNew[i * 3 + 2];
            }
        }

        public Matrix[] GetProjectedGaussSeidelSteps(int iterations)
        {
            var lambdaStep = this.initialLambda;
            Matrix[] lambdas = new Matrix[iterations];
            for (int i = 0; i < iterations; i++)
            {
                var lambda = ProjectedGaussSeidel.Solve(this.A, this.B, lambdaStep, this.minLambda, this.maxLambda, 1);
                lambdas[i] = lambda;
                lambdaStep = lambda;
            }
            return lambdas;
        }

        //Zur Testausgabe um zu sehen wie schnell PGS konvergiert
        private string GetProjectedGaussSeidelSteps(Matrix A, Matrix B, Matrix initialX, Matrix minX, Matrix maxX, int iterations)
        {
            string[] lambdas = new string[initialX.RowCount];

            for (int i=0;i< iterations;i++)
            {
                float[] l = ProjectedGaussSeidel.Solve(A, B, initialLambda, minLambda, maxLambda, i).GetColum(0);
                for (int j = 0; j < l.Length; j++)
                    lambdas[j] += String.Format("{0:+0.00;-0.00; 0.00}", l[j]) + (i < iterations - 1 ? "\t" : "");
            }

            return string.Join("\n", lambdas);
        }

        //Zur Testausgabe um zu sehen wie die Matrizen aussehen
        public string GetOutput()
        {
            return Testoutput(V, fExt, mInverse, bias, J, jTransposed, A, B, initialLambda, minLambda, maxLambda, lambda, vNew);
        }

        private static string Testoutput(Matrix V, Matrix fExt, Matrix mInverse, Matrix bias, Matrix J, Matrix jTransposed, Matrix A, Matrix B,
            Matrix initialLambda, Matrix minLambda, Matrix maxLambda, Matrix lambda, Matrix vNew)
        {
            string part1 =
                  "V=\n" + V
                + "\nfExt=\n" + fExt
                + "\nmInverse=\n" + mInverse
                + "\nbias=\n" + bias
                + "\nJ=\n" + J;                

            string Part3 =                  
                  "jTransposed=\n" + jTransposed
                + "\nA=\n" + A
                + "\nB=\n" + B
                + "\ninitialLambda=\n" + initialLambda
                + "\nminLambda=\n" + minLambda
                + "\nmaxLambda=\n" + maxLambda
                + "\nlambda=\n" + lambda
                + "\nvNew=\n" + vNew;

            return PrintTwoColums(part1, Part3);
        }

        private static string PrintTwoColums(string left, string right)
        {
            string[] lines1 = left.Replace("\r", "").Replace("+340282300000000000000000000000000000000,00", "Float.Max").Split(new char[] { '\r', '\n' });
            string[] lines2 = right.Replace("\r", "").Replace("+340282300000000000000000000000000000000,00", "Float.Max").Split(new char[] { '\r', '\n' });

            int lines = Math.Max(lines1.Length, lines2.Length);
            int leftWidth = lines1.Max(x => x.Length);

            int width = leftWidth + 10;

            StringBuilder str = new StringBuilder();
            for (int i = 0; i < lines; i++)
            {
                string s1 = i < lines1.Length ? lines1[i] : "";
                string s2 = i < lines2.Length ? lines2[i] : "";

                str.AppendLine(s1 + new string(' ', width - s1.Length) + s2);
            }

            return str.ToString();
        }
    }
}
