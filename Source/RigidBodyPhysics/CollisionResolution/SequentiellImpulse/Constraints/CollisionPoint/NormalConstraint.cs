using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.CollisionPoint
{
    //Verhindert, dass zwei Körper sich überlappen
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
            Vec2D p = ResolverHelper.GetContactPoint(c);
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

            Vec2D relativeVelocity = ResolverHelper.GetRelativeVelocityBetweenAnchorPointsWithoutNullCheck(c.B1, c.B2, r1, r2);

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
