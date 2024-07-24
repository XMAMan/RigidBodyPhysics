using GraphicMinimal;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    internal class NormalConstraint : IConstraint
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

        public bool IsSoftConstraint { get; } = false;
        public float Gamma { get; } = 0;

        public bool IsMultiConstraint { get; } = false;
        public Matrix2x2 InverseK { get; } = null;    //Wenn IsMultiConstraint=true, dann muss hier ein Wert stehen. K=J*M^-1*J^T
        public Vector2D GetCDot() => null; //Gibt CDot=J*V zurück (Geschwindigkeit in Richtung jeder J-Zeile)
        public Vector2D GetC() => null;  //Gibt den PositionError zurück
        public Vector2D AccumulatedMultiConstraintImpulse { get; set; }

        public float Beta { get; } = 0; //Soft-Multi-Constraint

        public float AccumulatedImpulse { get; set; }

        private CollisionPointWithImpulse point;
        public NormalConstraint(ConstraintConstructorData data, CollisionPointWithImpulse point) 
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
            float r1crossN = Vector2D.ZValueFromCross(this.R1, c.Normal);
            float r2crossN = Vector2D.ZValueFromCross(this.R2, c.Normal);

            this.ImpulseMass = 1.0f / (B1.InverseMass + B2.InverseMass +
                r1crossN * r1crossN * B1.InverseInertia +
                r2crossN * r2crossN * B2.InverseInertia);

            this.ForceDirection = c.Normal;
            this.Bias = GetBias(data, c, this.R1, this.R2);


            this.MinImpulse = 0;
            this.MaxImpulse = float.MaxValue;
            this.AccumulatedImpulse = data.Settings.DoWarmStart ? point.NormalImpulse : 0;
        }

        private float GetBias(ConstraintConstructorData data, CollisionPointWithImpulse c, Vector2D r1, Vector2D r2)
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

        public void SaveImpulse()
        {
            this.point.NormalImpulse = this.AccumulatedImpulse;
        }
    }
}
