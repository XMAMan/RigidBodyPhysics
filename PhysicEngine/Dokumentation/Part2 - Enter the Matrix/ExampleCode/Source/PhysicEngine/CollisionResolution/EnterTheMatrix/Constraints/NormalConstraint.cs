using GraphicMinimal;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix.Constraints
{
    internal class NormalConstraint : IConstraint
    {
        private ConstraintConstructorData data;
        public NormalConstraint(ConstraintConstructorData data)
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
                float r1crossN = Vector2D.ZValueFromCross(r1, c.Normal);
                float r2crossN = Vector2D.ZValueFromCross(r2, c.Normal);

                m[i1 * 3 + 0, y] = -c.Normal.X;
                m[i1 * 3 + 1, y] = -c.Normal.Y;
                m[i1 * 3 + 2, y] = -r1crossN;

                m[i2 * 3 + 0, y] = c.Normal.X;
                m[i2 * 3 + 1, y] = c.Normal.Y;
                m[i2 * 3 + 2, y] = r2crossN;
            }

            return new Matrix(m);
        }

        public Matrix GetBias()
        {
            float[] b = new float[this.data.Collisions.Length];

            float invDt = this.data.Dt > 0.0f ? 1.0f / this.data.Dt : 0.0f;
            var s = this.data.Settings;

            for (int y = 0; y < this.data.Collisions.Length; y++)
            {
                var c = this.data.Collisions[y];

                //Hebelarm bestimmen
                Vector2D start = c.Start * (c.B2.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
                Vector2D end = c.End * (c.B1.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
                Vector2D p = start + end;
                Vector2D r1 = p - c.B1.Center;
                Vector2D r2 = p - c.B2.Center;

                //VelocityAtContactPoint = V + mAngularVelocity cross R
                Vector2D v1 = c.B1.Velocity + new Vector2D(-c.B1.AngularVelocity * r1.Y, c.B1.AngularVelocity * r1.X);
                Vector2D v2 = c.B2.Velocity + new Vector2D(-c.B2.AngularVelocity * r2.Y, c.B2.AngularVelocity * r2.X);
                Vector2D relativeVelocity = v2 - v1;

                // Relative velocity in normal direction
                float velocityInNormal = relativeVelocity * c.Normal;
                float restituion = Math.Min(c.B1.Restituion, c.B2.Restituion);

                float restitutionBias = -restituion * velocityInNormal;

                float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;
                float positionBias = biasFactor * invDt * Math.Max(0, c.Depth - s.AllowedPenetration);

                b[y] = restitutionBias + positionBias;
            }

            return Matrix.GetColumVector(b);
        }

        public Matrix GetMinLambda()
        {
            return Matrix.GetColumVectorWithZeros(this.data.Collisions.Length);
        }

        public Matrix GetMaxLambda()
        {
            return Matrix.GetColumVectorWithInitialValue(this.data.Collisions.Length, float.MaxValue);
        }        
    }
}
