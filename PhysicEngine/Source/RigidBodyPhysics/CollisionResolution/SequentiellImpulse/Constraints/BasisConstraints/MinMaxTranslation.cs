using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.BasisConstraints
{
    //Stellt sicher, dass Body2 entlang der Achse von Body1 nur innerhalb der MinTranslation/MaxTranslation-Schranke sich bewegen kann
    internal class MinMaxTranslation : ILinear1DConstraint
    {
        public Vec2D R1 { get; } //Hebelarm vom B1.Center zum Ankerpunkt2 projetziert auf die B1.Center->Joint.Anchor1-Linie
        public Vec2D R2 { get; } //Hebelarm vom B2.Center zu Joint.Anchor2
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;

        public Vec2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        public float Bias { get; }
        public float ImpulseMass { get; } = 0; //Umrechungsvektor vom Relative-Kontaktpunktgeschwindigkeitswert in ein Impuls
        public float AccumulatedImpulse { get; set; }

        private IMinMaxTranslationJoint joint;

        public MinMaxTranslation(ConstraintConstructorData data, IMinMaxTranslationJoint joint)
        {
            this.joint = joint;
            var s = data.Settings;

            Vec2D d = joint.B1ToA2;

            B1 = joint.B1;
            B2 = joint.B2;
            R1 = d;
            R2 = joint.Anchor2 - joint.B2.Center;

            AccumulatedImpulse = s.DoWarmStart ? joint.AccumulatedMinMaxImpulse : 0;

            ForceDirection = joint.R1Dir;
            float s1 = Vec2D.ZValueFromCross(d, ForceDirection);
            float s2 = Vec2D.ZValueFromCross(R2, ForceDirection);

            float effectiveMass = 1.0f / (B1.InverseMass + s1 * s1 * B1.InverseInertia + B2.InverseMass + s2 * s2 * B2.InverseInertia);


            float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;

            float min = joint.R1Length * joint.MinTranslation; //Umrechung der Min-Max-Werte in Pixel
            float max = joint.R1Length * joint.MaxTranslation;
            float currentDistance = ForceDirection * d;

            //Wenn currentDistance im Bereich von min..max liegt, dann ist ImpulseMass=0 und somit wirkt gar kein Impuls

            if (currentDistance > max)
            {
                Bias = biasFactor * data.InvDt * (max - currentDistance);
                ImpulseMass = effectiveMass;
                MaxImpulse = 0; //Impuls soll nur ziehend wirken
            }

            if (currentDistance < min)
            {
                Bias = biasFactor * data.InvDt * (min - currentDistance);
                ImpulseMass = effectiveMass;
                MinImpulse = 0; //Impuls soll nur drückend wirken
            }
        }

        public void SaveImpulse()
        {
            joint.AccumulatedMinMaxImpulse = AccumulatedImpulse;
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
