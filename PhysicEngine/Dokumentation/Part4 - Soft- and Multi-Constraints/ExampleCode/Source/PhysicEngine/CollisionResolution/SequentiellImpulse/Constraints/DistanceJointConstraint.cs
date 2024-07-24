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
    //Stange oder Eisenfeder ohne Min-Max-Schranke
    internal class DistanceJointConstraint : IConstraint
    {
        //Schritt 1: Erzeuge über den Konstruktor folgende Propertys
        public Vector2D ForceDirection { get; } //In diese Richtung wird B2 gedrückt (B1 wird entgegengesetzt gedrückt)
        public Vector2D R1 { get; } //Hebelarm vom B1.Center zum Anchorpoint1
        public Vector2D R2 { get; } //Hebelarm vom B2.Center zum Anchorpoint2
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float Bias { get; }
        public float ImpulseMass { get; } //Umrechungsvektor vom Relative-Ankerpunktgeschwindigkeitswert in ein Impuls
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;
        
        public bool IsSoftConstraint { get; }
        public float Gamma { get; } = 0;

        public bool IsMultiConstraint { get; } = false;
        public Matrix2x2 InverseK { get; } = null;    //Wenn IsMultiConstraint=true, dann muss hier ein Wert stehen. K=J*M^-1*J^T
        public Vector2D GetCDot()=>null; //Gibt CDot=J*V zurück (Geschwindigkeit in Richtung jeder J-Zeile)
        public Vector2D GetC() => null;  //Gibt den PositionError zurück
        public Vector2D AccumulatedMultiConstraintImpulse { get; set; }

        public float Beta { get; } = 0; //Soft-Multi-Constraint

        public float AccumulatedImpulse { get; set; }

        private DistanceJoint joint;
        public DistanceJointConstraint(ConstraintConstructorData data, DistanceJoint joint)
        {
            this.joint = joint;
            var s = data.Settings;

            this.B1 = joint.B1;
            this.B2 = joint.B2;
            this.R1 = joint.Anchor1 - joint.B1.Center;
            this.R2 = joint.Anchor2 - joint.B2.Center;

            this.AccumulatedImpulse = s.DoWarmStart ? joint.AccumulatedImpulse : 0;

            Vector2D a1Toa2 = joint.Anchor2 - joint.Anchor1;
            float length = a1Toa2.Length();
            Vector2D n = length > 0.0001f ? a1Toa2 / length : new Vector2D(1, 0);

            this.ForceDirection = n;

            float r1crossN = Vector2D.ZValueFromCross(this.R1, n);
            float r2crossN = Vector2D.ZValueFromCross(this.R2, n);

            float invMass = B1.InverseMass + B1.InverseInertia * r1crossN * r1crossN + B2.InverseMass + B2.InverseInertia * r2crossN * r2crossN;
            this.ImpulseMass = 1f / invMass;

            float biasFactor = s.DoPositionalCorrection ? s.PositionalCorrectionRate : 0.0f;
            this.Bias = biasFactor * data.InvDt * (joint.Length - length);

            if (joint.ParameterType != ExportData.Joints.DistanceJointExportData.SpringParameter.NoSoftness)
            {
                this.IsSoftConstraint = true;
                float h = data.Dt;

                //Quelle1: https://github.com/erincatto/box2d/blob/main/src/dynamics/b2_distance_joint.cpp#L138C27-L138C27
                //Quelle2: 3D Constraint Derivations for Impulse Solvers - Marijn 2015" Seite 9 Formel (2.17)
                this.Gamma = 1.0f / (h * (joint.Damping + h * joint.Stiffness)); //Gamma entspricht den Gamma/dt-Wert aus (2.17) "3D Constraint Derivations for Impulse Solvers - Marijn 2015"
                this.Bias = (length - joint.Length) * h * joint.Stiffness * this.Gamma; //Bias entspricht den Bias*PositionError/dt-Wert aus (2.17)
                this.ImpulseMass = 1.0f / (invMass + this.Gamma); //ImpulseMass entspricht K aus der Formel (2.17)


                //So könnte man laut der Formel (2.13) oder (2.17) aus "3D Constraint Derivations for Impulse Solvers - Marijn 2015" auch den Impuls berechnen
                float gamma = 1 / (joint.Damping + h * joint.Stiffness);
                float beta = (h * joint.Stiffness) / (joint.Damping + h * joint.Stiffness);
                this.K = invMass + gamma / h;
                this.BetaCDeltaT = beta * (length - joint.Length) / h;
                this.GammaDt = gamma / h;

            }
            

        }

        //Testvariablen für Formel (2.13) und (2.17) aus "3D Constraint Derivations for Impulse Solvers - Marijn 2015"
        public float BetaCDeltaT;
        public float K;
        public float GammaDt;

        public void SaveImpulse()
        {
            this.joint.AccumulatedImpulse = this.AccumulatedImpulse;
        }
    }
}
