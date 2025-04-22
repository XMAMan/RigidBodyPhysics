using GraphicMinimal;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    //Diese Constraint verhindert, dass die DistanceJointConstraint zu lang/kurz wird (Wenn man sie als Feder mit kleinen Stiffness-Wert betreibt)
    internal class MinMaxDistanceConstraint : IConstraint
    {
        //Schritt 1: Erzeuge über den Konstruktor folgende Propertys
        public Vector2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        public Vector2D R1 { get; } //Hebelarm vom B1.Center zum Anchorpoint1
        public Vector2D R2 { get; } //Hebelarm vom B2.Center zum Anchorpoint2
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float Bias { get; }
        public float ImpulseMass { get; } = 0; //Umrechungsvektor vom Relative-Ankerpunktgeschwindigkeitswert in ein Impuls (0=Kein Impuls anwenden)
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;

        public bool IsSoftConstraint { get; } = false;
        public float Gamma { get; } = 0;

        public bool IsMultiConstraint { get; } = false;
        public Matrix2x2 InverseK { get; } = null;    //Wenn IsMultiConstraint=true, dann muss hier ein Wert stehen. K=J*M^-1*J^T
        public Vector2D GetCDot() => null; //Gibt CDot=J*V zurück (Geschwindigkeit in Richtung jeder J-Zeile)
        public Vector2D GetC() => null;  //Gibt den PositionError zurück
        public Vector2D AccumulatedMultiConstraintImpulse { get; set; }

        public float Beta { get; } = 0; //Soft-Multi-Constraint

        public float AccumulatedImpulse { get; set; }

        private DistanceJoint joint;

        public MinMaxDistanceConstraint(ConstraintConstructorData data, DistanceJoint joint)
        {
            this.joint = joint;
            var s = data.Settings;

            this.B1 = joint.B1;
            this.B2 = joint.B2;
            this.R1 = joint.Anchor1 - joint.B1.Center;
            this.R2 = joint.Anchor2 - joint.B2.Center;

            this.AccumulatedImpulse = s.DoWarmStart ? joint.AccumulatedImpulseForMinMax : 0;

            Vector2D a1Toa2 = joint.Anchor2 - joint.Anchor1;
            float length = a1Toa2.Length();
            Vector2D n = length > 0.0001f ? a1Toa2 / length : new Vector2D(1, 0);

            this.ForceDirection = n;

            float r1crossN = Vector2D.ZValueFromCross(this.R1, n);
            float r2crossN = Vector2D.ZValueFromCross(this.R2, n);

            float effectiveMass = 1f / (B1.InverseMass + B1.InverseInertia * r1crossN * r1crossN + B2.InverseMass + B2.InverseInertia * r2crossN * r2crossN);
            

            float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;

            float min = joint.Length * joint.MinLength;
            float max = joint.Length * joint.MaxLength;

            if (length > max)
            {
                this.Bias = biasFactor * data.InvDt * (max - length);
                this.ImpulseMass = effectiveMass;
                this.MaxImpulse = 0; //Impuls soll nur ziehend wirken
            }

            if (length < min)
            {
                this.Bias = biasFactor * data.InvDt * (min - length);
                this.ImpulseMass = effectiveMass;
                this.MinImpulse = 0; //Impuls soll nur drückend wirken
            }
        }


        public void SaveImpulse()
        {
            this.joint.AccumulatedImpulseForMinMax = this.AccumulatedImpulse;
        }
    }
}
