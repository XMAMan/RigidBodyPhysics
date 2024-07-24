using GraphicMinimal;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.JRowAsObject.Constraints
{
    internal class FrictionConstraint : IConstraint
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
            this.point.FrictionLambda = this.Lambda;
        }

        //........................
        private CollisionPointWithLambda point;

        public FrictionConstraint(ConstraintConstructorData data, CollisionPointWithLambda point)
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
            Vector2D tangent = Vector2D.CrossWithZ(c.Normal, 1.0f);
            float r1crossT = Vector2D.ZValueFromCross(r1, tangent);
            float r2crossT = Vector2D.ZValueFromCross(r2, tangent);

            //J-Zeile
            this.J1 = new Vector3D(-tangent.X, -tangent.Y, -r1crossT);
            this.J2 = new Vector3D(tangent.X, tangent.Y, r2crossT);
            this.JRow = new Vector3D[data.Bodies.Count];
            this.JRow[this.Body1Index] = this.J1;
            this.JRow[this.Body2Index] = this.J2;

            //Bias
            this.Bias = 0;

            //Lambda
            float friction = Math.Max(c.B1.Friction, c.B2.Friction);
            this.MaxLambda = data.Settings.Gravity * friction * 0.15f;
            this.MinLambda = -this.MaxLambda;
            this.Lambda = data.Settings.DoWarmStart ? point.FrictionLambda : 0;
        }
    }
}
