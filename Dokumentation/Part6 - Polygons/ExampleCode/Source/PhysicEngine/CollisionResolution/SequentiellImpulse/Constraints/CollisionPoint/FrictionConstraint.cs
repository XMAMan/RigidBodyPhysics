using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints.CollisionPoint
{
    internal class FrictionConstraint : ILinear1DConstraint
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

        public FrictionConstraint(ConstraintConstructorData data, CollisionPointWithImpulse point)
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
            Vec2D tangent = Vec2D.CrossWithZ(c.Normal, 1.0f);
            float r1crossT = Vec2D.ZValueFromCross(R1, tangent);
            float r2crossT = Vec2D.ZValueFromCross(R2, tangent);

            ImpulseMass = 1.0f / (B1.InverseMass + B2.InverseMass +
                r1crossT * r1crossT * B1.InverseInertia +
                r2crossT * r2crossT * B2.InverseInertia);

            ForceDirection = tangent;
            Bias = 0;


            float friction = Math.Max(c.B1.Friction, c.B2.Friction);
            MaxImpulse = c.NormalImpulse * friction;
            MinImpulse = -MaxImpulse;

            AccumulatedImpulse = data.Settings.DoWarmStart ? point.FrictionImpulse : 0;
        }

        public void SaveImpulse()
        {
            point.FrictionImpulse = AccumulatedImpulse;
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
