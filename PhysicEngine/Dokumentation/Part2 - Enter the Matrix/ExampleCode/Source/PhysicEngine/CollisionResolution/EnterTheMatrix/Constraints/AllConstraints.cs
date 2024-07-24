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
    }
}
