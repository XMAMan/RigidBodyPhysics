using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.CollisionResolution.SequentiellImpulse.Constraints.BasisConstraints
{
    //Hält die beiden Ankerpunkte fix zusammen
    internal class PointToPoint : I2DConstraint
    {
        //Schritt 1: Erzeuge über den Konstruktor folgende Propertys
        public Vec2D R1 { get; } //Hebelarm vom B1.Center zum Anchorpoint1
        public Vec2D R2 { get; } //Hebelarm vom B2.Center zum Anchorpoint2
        public IRigidBody B1 { get; }
        public IRigidBody B2 { get; }
        public float MinImpulse { get; } = float.MinValue;
        public float MaxImpulse { get; } = float.MaxValue;

        public Matrix2x2 InverseK { get; } = null;    //Wenn IsMultiConstraint=true, dann muss hier ein Wert stehen. K=J*M^-1*J^T
        public Vec2D GetCDot() //Gibt CDot=J*V zurück (Geschwindigkeit in Richtung jeder J-Zeile)
        {
            //Variante 1: Relative Ankerpunktgeschwindigkeit über Drehhebelarmformel: V + Cross([0, 0, AngularVelocity], [R.X, R.Y, 0])
            Vec2D v1 = B1.Velocity + new Vec2D(-B1.AngularVelocity * R1.Y, B1.AngularVelocity * R1.X);
            Vec2D v2 = B2.Velocity + new Vec2D(-B2.AngularVelocity * R2.Y, B2.AngularVelocity * R2.X);
            Vec2D cDot = v2 - v1;

            //Variante 2: J*V-Formel
            //float cDotX = -B1.Velocity.X + R1.Y*B1.AngularVelocity + B2.Velocity.X - R2.Y*B2.AngularVelocity;
            //float cDotY = -B1.Velocity.Y - R1.X*B1.AngularVelocity + B2.Velocity.Y + R2.X*B2.AngularVelocity;

            return cDot;
        }
        public Vec2D Bias { get; }
        public Vec2D AccumulatedImpulse { get; set; } = new Vec2D(0, 0);


        private IPointToPointJoint joint;

        public PointToPoint(ConstraintConstructorData data, IPointToPointJoint joint)
        {
            this.joint = joint;

            this.B1 = joint.B1;
            this.B2 = joint.B2;
            this.R1 = joint.Anchor1 - joint.B1.Center;
            this.R2 = joint.Anchor2 - joint.B2.Center;

            var s = data.Settings;
            this.AccumulatedImpulse = s.DoWarmStart ? joint.AccumulatedPointToPointImpulse : new Vec2D(0, 0);

            //this.Bias = s.PositionalCorrectionRate * data.InvDt * (joint.Anchor2 - joint.Anchor1);
            this.Bias = 1 * data.InvDt * (joint.Anchor2 - joint.Anchor1); //Wenn ich eine PositionalCorrectionRate von 0.2 anstatt 1 verwende, dann braucht Point2Point viele Frames um das Bein vom Runner zusammen zu halten

            float x = -R1.X * R1.Y * B1.InverseInertia - R2.X * R2.Y * B2.InverseInertia;

            this.InverseK = Matrix2x2.FromScalars(
                    B1.InverseMass + B2.InverseMass + R1.Y * R1.Y * B1.InverseInertia + R2.Y * R2.Y * B2.InverseInertia,
                    x,
                    x,
                    B1.InverseMass + B2.InverseMass + R1.X * R1.X * B1.InverseInertia + R2.X * R2.X * B2.InverseInertia
                )
                .Invert();
        }

        public void SaveImpulse()
        {
            this.joint.AccumulatedPointToPointImpulse = this.AccumulatedImpulse;
        }

        public void ApplyWarmStartImpulse()
        {
            ResolverHelper.ApplyLinearImpulse(this, this.AccumulatedImpulse);
        }
        public void DoSingleSIStep()
        {
            ResolverHelper.DoSingleSIStepStiff(this);
        }
    }
}
