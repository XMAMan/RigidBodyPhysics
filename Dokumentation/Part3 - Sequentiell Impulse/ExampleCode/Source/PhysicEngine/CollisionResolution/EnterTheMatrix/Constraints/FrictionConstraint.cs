using GraphicMinimal;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix.Constraints
{
    internal class FrictionConstraint : IConstraint
    {
        private ConstraintConstructorData data;
        public FrictionConstraint(ConstraintConstructorData data)
        {
            this.data = data;
        }

        public Matrix GetJacobian()
        {
            float[,] m = new float[3 * this.data.Bodies.Count, this.data.Collisions.Length];

            for (int y = 0; y < this.data.Collisions.Length; y++)
            {
                var c = this.data.Collisions[y];
                int i1 = this.data.Bodies.IndexOf(c.B1);
                int i2 = this.data.Bodies.IndexOf(c.B2);

                //Hebelarm bestimmen
                Vector2D start = c.Start * (c.B2.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
                Vector2D end = c.End * (c.B1.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
                Vector2D p = start + end;
                Vector2D r1 = p - c.B1.Center;
                Vector2D r2 = p - c.B2.Center;

                Vector2D tangent = Vector2D.CrossWithZ(c.Normal, 1.0f);
                float r1crossT = Vector2D.ZValueFromCross(r1, tangent);
                float r2crossT = Vector2D.ZValueFromCross(r2, tangent);

                m[i1 * 3 + 0, y] = -tangent.X;
                m[i1 * 3 + 1, y] = -tangent.Y;
                m[i1 * 3 + 2, y] = -r1crossT;

                m[i2 * 3 + 0, y] = tangent.X;
                m[i2 * 3 + 1, y] = tangent.Y;
                m[i2 * 3 + 2, y] = r2crossT;
            }

            return new Matrix(m);
        }

        public Matrix GetBias()
        {
            return Matrix.GetColumVectorWithInitialValue(this.data.Collisions.Length, 0);
        }

        public Matrix GetMinLambda()
        {
            return GetMaxLambda() * -1;
        }

        public Matrix GetMaxLambda()
        {
            float[] b = new float[this.data.Collisions.Length];

            for (int y = 0; y < this.data.Collisions.Length; y++)
            {
                var c = this.data.Collisions[y];
                float friction = Math.Max(c.B1.Friction, c.B2.Friction);

                b[y] = this.data.Settings.Gravity * friction * 0.15f;
            }

            return Matrix.GetColumVector(b);
        }

		public Matrix GetInitialLambda()
		{
			return Matrix.GetColumVector(this.data.Collisions.Select(x => x.FrictionLambda).ToArray());
		}
		public int GetLambdaRowCount()
		{
			return this.data.Collisions.Length;
		}
		public void SaveLambdaInCollisionPoints(Matrix lambda) //Speichert die Lambdawerte in CollisionPointWithLambda
		{
			for (int i = 0; i < lambda.RowCount; i++)
			{
				this.data.Collisions[i].FrictionLambda = lambda[0, i];
			}
		}
	}
}
