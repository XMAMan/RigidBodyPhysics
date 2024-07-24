using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.CollisionPoint
{
    internal class NormalConstraint : ILinear1DConstraint
    {
        public Vec2D R1 { get; } //Hebelarm vom B1.Center zum Kontaktpunkt
        public Vec2D R2 { get; } //Hebelarm vom B2.Center zum Kontaktpunkt
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float MinImpulse { get; }
        public float MaxImpulse { get; }

        public Vec2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        public float Bias { get; }
        public float ImpulseMass { get; } //Umrechungsvektor vom Relative-Kontaktpunktgeschwindigkeitswert in ein Impuls
        public float AccumulatedImpulse { get; set; }


        private CollisionPointWithImpulse point;
        public NormalConstraint(ConstraintConstructorData data, CollisionPointWithImpulse point)
        {
            this.point = point;
            B1 = point.B1;
            B2 = point.B2;

            var c = point;

            //Hebelarm bestimmen
            Vec2D start = c.Start * (c.B2.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
            Vec2D end = c.End * (c.B1.InverseMass / (c.B1.InverseMass + c.B2.InverseMass));
            Vec2D p = start + end;
            R1 = p - c.B1.Center;
            R2 = p - c.B2.Center;
            float r1crossN = Vec2D.ZValueFromCross(R1, c.Normal);
            float r2crossN = Vec2D.ZValueFromCross(R2, c.Normal);

            ImpulseMass = 1.0f / (B1.InverseMass + B2.InverseMass +
                r1crossN * r1crossN * B1.InverseInertia +
                r2crossN * r2crossN * B2.InverseInertia);

            ForceDirection = c.Normal;
            Bias = GetBias(data, c, R1, R2);


            MinImpulse = 0;
            MaxImpulse = float.MaxValue;
            AccumulatedImpulse = data.Settings.DoWarmStart ? point.NormalImpulse : 0;
        }

        private float GetBias(ConstraintConstructorData data, CollisionPointWithImpulse c, Vec2D r1, Vec2D r2)
        {
            var s = data.Settings;

            //VelocityAtContactPoint = V + mAngularVelocity cross R
            Vec2D v1 = c.B1.Velocity + new Vec2D(-c.B1.AngularVelocity * r1.Y, c.B1.AngularVelocity * r1.X);
            Vec2D v2 = c.B2.Velocity + new Vec2D(-c.B2.AngularVelocity * r2.Y, c.B2.AngularVelocity * r2.X);
            Vec2D relativeVelocity = v2 - v1;

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
            point.NormalImpulse = AccumulatedImpulse;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyWarmStartImpulse(this);
        }
        public void DoSingleSIStep()
        {
            ResolverHelper.DoSingleSIStepStiff(this);
        }
    }
}
