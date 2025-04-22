using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.AxialFriction
{
    //Reibung welche an ein Ankerpunkt von ein Körper in eine vorgegebene Axenrichtung wirkt
    internal class AxialFrictionConstraint : ILinear1DConstraint
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


        private IAxialFriction axialFriction;

        public AxialFrictionConstraint(ConstraintConstructorData data, IAxialFriction axialFriction)
        {
            this.axialFriction = axialFriction;
            B1 = axialFriction.B1;
            B2 = null;

            R1 = axialFriction.R1;
            R2 = null;
            float r1crossT = Vec2D.ZValueFromCross(R1, axialFriction.ForceDirection);

            ImpulseMass = 1.0f / (B1.InverseMass +
                r1crossT * r1crossT * B1.InverseInertia);

            ForceDirection = axialFriction.ForceDirection;
            Bias = 0;


            MaxImpulse = axialFriction.Friction * data.Dt;
            MinImpulse = -MaxImpulse;

            AccumulatedImpulse = data.Settings.DoWarmStart ? axialFriction.AccumulatedFrictionImpulse : 0;
        }

        public void SaveImpulse()
        {
            axialFriction.AccumulatedFrictionImpulse = AccumulatedImpulse;
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
