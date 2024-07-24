using GraphicMinimal;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    //Körper, der per Maus angeklickt wurde und festegehalten wird
    internal class MouseConstraint : IConstraint
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

        public bool IsSoftConstraint { get; } = false;
        public float Gamma { get; } = 0;

        public bool IsMultiConstraint { get; } = true;
        public Matrix2x2 InverseK { get; } = null;    //Wenn IsMultiConstraint=true, dann muss hier ein Wert stehen. K=J*M^-1*J^T
        public Vector2D GetCDot() //Gibt CDot=J*V zurück (Geschwindigkeit in Richtung jeder J-Zeile)
        {
            var b = this.mouseData.RigidBody;
            Vector2D v = b.Velocity + new Vector2D(-b.AngularVelocity * this.R2.Y, b.AngularVelocity * this.R2.X);
            return v;
        }
        public Vector2D GetC()
        {
            var b = this.mouseData.RigidBody;
            Vector2D positionError = b.Center + this.R2 - this.mouseData.MousePosition;

            return positionError;
        }
        public Vector2D AccumulatedMultiConstraintImpulse { get; set; } = new Vector2D(0, 0);

        public float Beta { get; } = 0; //Soft-Multi-Constraint

        public float AccumulatedImpulse { get; set; }

        private MouseConstraintData mouseData;

        public MouseConstraint(ConstraintConstructorData data, MouseConstraintData mouseData)
        {
            this.mouseData = mouseData;

            this.B2 = mouseData.RigidBody;
            this.B1 = null;
            this.R2 = MathHelp.GetWorldDirectionFromLocalDirection(mouseData.RigidBody, mouseData.LocalAnchorDirection);
            this.R1 = null;

            this.AccumulatedMultiConstraintImpulse = data.Settings.DoWarmStart ? mouseData.AccumulatedImpulse : new Vector2D(0, 0);

            var b = mouseData.RigidBody;
            var r = this.R2;

            this.MaxImpulse = data.Dt * mouseData.MaxForce;

            if (mouseData.IsSoftConstraint == false)
            {
                this.InverseK = Matrix2x2.FromScalars(
                    b.InverseMass + r.Y * r.Y * b.InverseInertia,
                    -r.X * r.Y * b.InverseInertia,
                    -r.X * r.Y * b.InverseInertia,
                    b.InverseMass + r.X * r.X * b.InverseInertia
                )
                .Invert();
            }else
            {
                this.IsSoftConstraint = true;

                float h = data.Dt;
                float d = mouseData.Damping;
                float k = mouseData.Stiffness;
                this.Gamma = h * (d + h * k);   
                if (this.Gamma != 0)
                    this.Gamma = 1.0f / this.Gamma;     //Gamma / h
                this.Beta = h * k * this.Gamma;         //Beta / h

                this.InverseK = Matrix2x2.FromScalars(
                    b.InverseMass + r.Y * r.Y * b.InverseInertia + this.Gamma,
                    -r.X * r.Y * b.InverseInertia,
                    -r.X * r.Y * b.InverseInertia,
                    b.InverseMass + r.X * r.X * b.InverseInertia + this.Gamma
                )
                .Invert();
            }          
        }

        public void SaveImpulse()
        {
            mouseData.AccumulatedImpulse = this.AccumulatedMultiConstraintImpulse;
        }
    }
}
