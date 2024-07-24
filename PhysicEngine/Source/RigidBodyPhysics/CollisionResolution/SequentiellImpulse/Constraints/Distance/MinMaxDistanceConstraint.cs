using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.Distance
{
    //Diese Constraint verhindert, dass die DistanceJointConstraint zu lang/kurz wird (Wenn man sie als Feder mit kleinen Stiffness-Wert betreibt)
    //Ich kann hier nicht die MinMaxTranslation-Klasse nehmen, da die Achse, auf der sich Body2 bewegt über Anchor1->Anchor2 definiert wird und nicht über R1
    internal class MinMaxDistanceConstraint : ILinear1DConstraint
    {
        public Vec2D R1 { get; } //Hebelarm vom B1.Center zum Anchorpoint1
        public Vec2D R2 { get; } //Hebelarm vom B2.Center zum Anchorpoint2
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;

        public Vec2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        public float Bias { get; } = 0;
        public float ImpulseMass { get; } = 0; //Umrechungsvektor vom Relative-Ankerpunktgeschwindigkeitswert in ein Impuls (0=Kein Impuls anwenden)
        public float AccumulatedImpulse { get; set; }


        private DistanceJoint joint;

        public MinMaxDistanceConstraint(ConstraintConstructorData data, DistanceJoint joint)
        {
            this.joint = joint;
            var s = data.Settings;

            B1 = joint.B1;
            B2 = joint.B2;
            R1 = joint.Anchor1 - joint.B1.Center;
            R2 = joint.Anchor2 - joint.B2.Center;

            AccumulatedImpulse = s.DoWarmStart ? joint.AccumulatedImpulseForMinMax : 0;

            Vec2D a1Toa2 = joint.Anchor2 - joint.Anchor1;
            float length = a1Toa2.Length();
            Vec2D n = length > 0.0001f ? a1Toa2 / length : new Vec2D(1, 0);

            ForceDirection = n;

            float r1crossN = Vec2D.ZValueFromCross(R1, n);
            float r2crossN = Vec2D.ZValueFromCross(R2, n);

            float effectiveMass = 1f / (B1.InverseMass + B1.InverseInertia * r1crossN * r1crossN + B2.InverseMass + B2.InverseInertia * r2crossN * r2crossN);


            float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;

            float min = joint.MinLength;
            float max = joint.JointIsRope ? joint.LengthPosition : joint.MaxLength;

            if (length > max)
            {
                Bias = biasFactor * data.InvDt * (max - length);
                ImpulseMass = effectiveMass;
                MaxImpulse = 0; //Impuls soll nur ziehend wirken
            }

            if (length < min)
            {
                Bias = biasFactor * data.InvDt * (min - length);
                ImpulseMass = effectiveMass;
                MinImpulse = 0; //Impuls soll nur drückend wirken
            }
        }


        public void SaveImpulse()
        {
            joint.AccumulatedImpulseForMinMax = AccumulatedImpulse;
        }

        public void ApplyWarmStartImpulse()
        {
            //Wenn man bei Rope den Warmstart nutzt, dann geht das Gewicht nicht nach unten, wenn die Seillänge sich erhöht
            if (joint.JointIsRope == false)
                ResolverHelper.ApplyWarmStartImpulse(this);
        }
        public void DoSingleSIStep()
        {
            ResolverHelper.DoSingleSIStepStiff(this);
        }
    }
}
