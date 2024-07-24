using GraphicMinimal;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.JRowAsObject.Constraints
{
    internal class NormalConstraint : IConstraint
    {
        //Schritt 1: Erzeuge über den Konstruktor folgende Propertys
        public Vector3D J1 { get; } //Die 3 J-Werte von Körper 1
        public Vector3D J2 { get; } //Die 3 J-Werte von Körper 2
        public Vector3D[] JRow { get; } //Array der Länge Bodies.Length wo nur 2 Einträge ein Wert haben
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public int Body1Index { get; }
        public int Body2Index { get; }
        public float Bias { get; }
        public float MinLambda { get; }
        public float MaxLambda { get; }

        
        public float[] ARow { get; set; } //Zeile aus der A-Matrix
        public float B { get; set; } //Einzelner Wert aus dem B-Spaltenvektor


        //Schritt 3: Berechne per PGS Lambda
        public float Lambda { get; set; }

        public void SaveLambda() //Speichert das Lambda im CollisionPointWithLambda
        {
            this.point.NormalLambda = this.Lambda;
        }


        //........................
        private CollisionPointWithLambda point;
        public NormalConstraint(ConstraintConstructorData data, CollisionPointWithLambda point)
        {
            this.point = point;
            this.B1 = point.B1;
            this.B2 = point.B2;
            this.Body1Index = data.Bodies.IndexOf(B1);
            this.Body2Index = data.Bodies.IndexOf(B2);

            var c = point;

            //Hebelarm bestimmen
            Vector2D start = c.Start * (c.B2.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
            Vector2D end = c.End * (c.B1.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
            Vector2D p = start + end;
            Vector2D r1 = p - c.B1.Center;
            Vector2D r2 = p - c.B2.Center;
            float r1crossN = Vector2D.ZValueFromCross(r1, c.Normal);
            float r2crossN = Vector2D.ZValueFromCross(r2, c.Normal);

            //J-Zeile
            this.J1 = new Vector3D(-c.Normal.X, -c.Normal.Y, -r1crossN);
            this.J2 = new Vector3D(c.Normal.X, c.Normal.Y, r2crossN);
            this.JRow = new Vector3D[data.Bodies.Count];
            this.JRow[this.Body1Index] = this.J1;
            this.JRow[this.Body2Index] = this.J2;

            //Bias
            this.Bias = GetBias(data, c, r1, r2);

            //Lambda
            this.MinLambda = 0;
            this.MaxLambda = float.MaxValue;
            this.Lambda = data.Settings.DoWarmStart ? point.NormalLambda : 0;
        }

        private float GetBias(ConstraintConstructorData data, CollisionPointWithLambda c, Vector2D r1, Vector2D r2)
        {
            var s = data.Settings;

            //VelocityAtContactPoint = V + mAngularVelocity cross R
            Vector2D v1 = c.B1.Velocity + new Vector2D(-c.B1.AngularVelocity * r1.Y, c.B1.AngularVelocity * r1.X);
            Vector2D v2 = c.B2.Velocity + new Vector2D(-c.B2.AngularVelocity * r2.Y, c.B2.AngularVelocity * r2.X);
            Vector2D relativeVelocity = v2 - v1;

            // Relative velocity in normal direction
            float velocityInNormal = relativeVelocity * c.Normal;
            float restituion = Math.Min(c.B1.Restituion, c.B2.Restituion);

            float restitutionBias = -restituion * velocityInNormal;

            float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;
            float positionBias = biasFactor * data.InvDt * Math.Max(0, c.Depth - s.AllowedPenetration);

            return restitutionBias + positionBias;
        }
    }
}
