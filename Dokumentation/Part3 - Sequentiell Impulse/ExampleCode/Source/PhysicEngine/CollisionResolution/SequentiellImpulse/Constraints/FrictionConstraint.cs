using GraphicMinimal;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    internal class FrictionConstraint : IConstraint
    {
        //Schritt 1: Erzeuge über den Konstruktor folgende Propertys
        public Vector2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        public Vector2D R1 { get; } //Hebelarm vom B1.Center zum Kontaktpunkt
        public Vector2D R2 { get; } //Hebelarm vom B2.Center zum Kontaktpunkt
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float Bias { get; }
        public float ImpulseMass { get; } //Umrechungsvektor vom Relative-Kontaktpunktgeschwindigkeitswert in ein Impuls
        public float MinImpulse { get; }
        public float MaxImpulse { get; }
        public float AccumulatedImpulse { get; set; }

        private CollisionPointWithImpulse point;

        public FrictionConstraint(ConstraintConstructorData data, CollisionPointWithImpulse point)
        {
            this.point = point;
            this.B1 = point.B1;
            this.B2 = point.B2;

            var c = point;

            //Hebelarm bestimmen
            Vector2D start = c.Start * (c.B2.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
            Vector2D end = c.End * (c.B1.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
            Vector2D p = start + end;
            this.R1 = p - c.B1.Center;
            this.R2 = p - c.B2.Center;
            Vector2D tangent = Vector2D.CrossWithZ(c.Normal, 1.0f);
            float r1crossT = Vector2D.ZValueFromCross(this.R1, tangent);
            float r2crossT = Vector2D.ZValueFromCross(this.R2, tangent);

            this.ImpulseMass = 1.0f / (B1.InverseMass + B2.InverseMass +
                r1crossT * r1crossT * B1.InverseInertia +
                r2crossT * r2crossT * B2.InverseInertia);

            this.ForceDirection = tangent;
            this.Bias = 0;


            float friction = Math.Max(c.B1.Friction, c.B2.Friction);
            this.MaxImpulse = data.Settings.Gravity * friction * 0.15f * data.Dt;
            this.MinImpulse = -this.MaxImpulse;

            this.AccumulatedImpulse = data.Settings.DoWarmStart ? point.FrictionImpulse : 0;
        }

        public void SaveImpulse()
        {
            this.point.FrictionImpulse = this.AccumulatedImpulse;
        }
    }
}
