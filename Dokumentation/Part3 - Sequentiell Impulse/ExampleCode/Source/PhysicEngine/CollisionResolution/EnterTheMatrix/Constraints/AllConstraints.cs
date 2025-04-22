namespace PhysicEngine.CollisionResolution.EnterTheMatrix.Constraints
{
    //Erstellt für jedes Constraint eine Matrix und schreibt dann alle Zeilen von den jeweiligen Matrizen untereinander
    internal class AllConstraints : IConstraint
    {
        private IConstraint[] constraints;
        public AllConstraints(ConstraintConstructorData data)
        {
            this.constraints = new IConstraint[]
            {
                new NormalConstraint(data),
                new FrictionConstraint(data),
            };
        }

        public Matrix GetJacobian()
        {
            return Matrix.CreateFromMany(this.constraints.Select(x => x.GetJacobian()).ToArray());
        }

        public Matrix GetBias()
        {
            return Matrix.CreateFromMany(this.constraints.Select(x => x.GetBias()).ToArray());
        }



        public Matrix GetMinLambda()
        {
            return Matrix.CreateFromMany(this.constraints.Select(x => x.GetMinLambda()).ToArray());
        }

        public Matrix GetMaxLambda()
        {
            return Matrix.CreateFromMany(this.constraints.Select(x => x.GetMaxLambda()).ToArray());
        }

		public Matrix GetInitialLambda()
		{
			return Matrix.CreateFromMany(this.constraints.Select(x => x.GetInitialLambda()).ToArray());
		}
		public int GetLambdaRowCount()
		{
            return this.constraints.Sum(x => x.GetLambdaRowCount());
		}
		public void SaveLambdaInCollisionPoints(Matrix lambda) //Speichert die Lambdawerte in CollisionPointWithLambda
		{
            int y = 0;
            for (int i=0;i<this.constraints.Length;i++)
            {
                var subLambda = lambda.GetRange(0, y, 1, this.constraints[i].GetLambdaRowCount());
                this.constraints[i].SaveLambdaInCollisionPoints(subLambda);
                y += subLambda.RowCount;
			}
		}
	}
}
