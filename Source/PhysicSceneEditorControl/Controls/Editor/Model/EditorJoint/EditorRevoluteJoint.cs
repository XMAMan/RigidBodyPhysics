using GraphicPanels;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using PhysicSceneEditorControl.Controls.JointPropertys.RevoluteJoint;
using PhysicSceneEditorControl.Controls.SoftProperty;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.EditorJoint
{
    internal class EditorRevoluteJoint : IEditorJoint
    {
        private Vec2D r1;
        private Vec2D r2;

        private Vec2D anchorWorldPosition1;
        private Vec2D anchorWorldPosition2;
        private float circleForP1 = 10; //Diesen Abstand haben die P1-Punkte zum Center
        private float limitArmLength = 80; //Diesen Abstand haben die P1-Punkte zum Center

        private Vec2D leverArmAP1, leverArmBP1;//Arm von Center Body1.Center/Body2.Center
        private Vec2D limitArmA, limitArmB;    //Richtungsvektor

        public IEditorShape Body1 { get; }
        public IEditorShape Body2 { get; }
        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Blue;

        public bool SupportsDefineLimit { get; } = true;

        public RevoluteJointPropertyViewModel Properties { get; }

        public bool DrawLimits = false;
        public Pen LimitArmAPen { get; set; } = Pens.Blue;
        public Pen LimitArmBPen { get; set; } = Pens.Blue;

        //Neu anlegen
        public EditorRevoluteJoint(IEditorShape body1, IEditorShape body2, Vec2D r1, Vec2D r2)
        {
            this.Body1 = body1;
            this.Body2 = body2;
            this.r1 = r1;
            this.r2 = r2;


            this.Properties = new RevoluteJointPropertyViewModel();
            UpdateAfterMovingBodys();
        }

        //Aus Datei einladen
        public EditorRevoluteJoint(RevoluteJointExportData ctor, List<IEditorShape> shapes)
        {
            this.Body1 = shapes[ctor.BodyIndex1];
            this.Body2 = shapes[ctor.BodyIndex2];
            this.r1 = ctor.R1;
            this.r2 = ctor.R2;

            this.Properties = new RevoluteJointPropertyViewModel()
            {
                CollideConnected = ctor.CollideConnected,
                LimitIsEnabled = ctor.LimitIsEnabled,
                LowerAngle = ctor.LowerAngle,
                UpperAngle = ctor.UpperAngle,
                Motor = ctor.Motor,
                MotorSpeed = ctor.MotorSpeed,
                MaxMotorTorque = ctor.MaxMotorTorque,
                Soft = new SoftPropertyViewModel(ctor.SoftData),
                BreakWhenMaxForceIsReached = ctor.BreakWhenMaxForceIsReached,
                MaxForceToBreak = ctor.MaxForceToBreak,
            };
            UpdateAfterMovingBodys();
        }

        public void UpdateAfterMovingBodys()
        {
            this.anchorWorldPosition1 = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body1, this.r1);
            this.anchorWorldPosition2 = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body2, this.r2);

            Vec2D r1 = (this.Body1.Center - this.anchorWorldPosition1).Normalize();
            Vec2D r2 = (this.Body2.Center - this.anchorWorldPosition1).Normalize();

            this.leverArmAP1 = this.anchorWorldPosition1 + r1 * this.circleForP1;
            this.leverArmBP1 = this.anchorWorldPosition1 + r2 * this.circleForP1;

            this.limitArmA = Vec2D.GetV2FromAngle360(r1, this.Properties.LowerAngle);
            this.limitArmB = Vec2D.GetV2FromAngle360(r1, this.Properties.UpperAngle);
        }

        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            if (this.Backcolor != Color.Transparent)
            {
                panel.DrawFillCircle(this.Backcolor, this.anchorWorldPosition1.ToGrx(), 5);
                panel.DrawLine(new Pen(this.Backcolor, 3), this.leverArmAP1.ToGrx(), this.Body1.Center.ToGrx());
                panel.DrawLine(new Pen(this.Backcolor, 3), this.leverArmBP1.ToGrx(), this.Body2.Center.ToGrx());
            }

            panel.DrawLine(this.BorderPen, this.leverArmAP1.ToGrx(), this.Body1.Center.ToGrx());
            panel.DrawLine(this.BorderPen, this.leverArmBP1.ToGrx(), this.Body2.Center.ToGrx());
            panel.DrawCircle(this.BorderPen, this.anchorWorldPosition1.ToGrx(), this.circleForP1);

            //Testausgabe
            //if (IsAngleInMinMaxRange(GetLimitAngle(this.Body2.Center)))
            //    panel.DrawString(Center.ToGrx(), Color.Green, 30, "JA");
            //else
            //    panel.DrawString(Center.ToGrx(), Color.Red, 30, "NEIN");



            panel.DrawFillCircle(this.BorderPen.Color, this.anchorWorldPosition1.ToGrx(), 2);

            if (this.DrawLimits && this.Properties.LimitIsEnabled)
            {
                Vec2D limitAP1 = this.anchorWorldPosition1 + this.limitArmA * this.circleForP1;
                Vec2D limitAP2 = this.anchorWorldPosition1 + this.limitArmA * this.limitArmLength;
                panel.DrawLine(this.LimitArmAPen, limitAP1.ToGrx(), limitAP2.ToGrx());

                Vec2D limitBP1 = this.anchorWorldPosition1 + this.limitArmB * this.circleForP1;
                Vec2D limitBP2 = this.anchorWorldPosition1 + this.limitArmB * this.limitArmLength;
                panel.DrawLine(this.LimitArmBPen, limitBP1.ToGrx(), limitBP2.ToGrx());

                float lowerAngle = Vec2D.Angle360YMirrored(new Vec2D(1, 0), this.limitArmA);
                float upperAngle = Vec2D.Angle360YMirrored(new Vec2D(1, 0), this.limitArmB);
                panel.DrawCircleArc(new Pen(Color.Yellow, 5), this.anchorWorldPosition1.ToGrx(), (int)this.limitArmLength, upperAngle, lowerAngle, false);

                panel.DrawStringOnCircleBorder("1", 20, Color.Green, this.Body1.Center, (this.Body1.Center - this.anchorWorldPosition1).Normalize());
                panel.DrawStringOnCircleBorder("2", 20, Color.Green, this.Body2.Center, (this.Body2.Center - this.anchorWorldPosition1).Normalize());
                panel.DrawStringOnCircleBorder("Min", 20, Color.Black, limitAP2, limitArmA.Normalize());
                panel.DrawStringOnCircleBorder("Max", 20, Color.Black, limitBP2, limitArmB.Normalize());

                //Testausgabe
                //float b2 = GetLimitAngle(this.Body2.Center);
                //panel.DrawStringOnCircleBorder("Pos " + (int)b2, 20, Color.Green, this.anchorWorldPosition1 + (this.Body2.Center - this.anchorWorldPosition1).Normalize() * limitArmLength, (this.Body2.Center - this.anchorWorldPosition1).Normalize());
                //panel.DrawStringOnCircleBorder("Min " + (int)this.Properties.LowerAngle, 20, Color.Black, limitAP2, limitArmA.Normalize());
                //panel.DrawStringOnCircleBorder("Max " + (int)this.Properties.UpperAngle, 20, Color.Black, limitBP2, limitArmB.Normalize());

            }
        }



        private bool IsAngleInMinMaxRange(float angle)
        {
            if (this.Properties.LowerAngle < this.Properties.UpperAngle)
                return angle >= this.Properties.LowerAngle && angle <= this.Properties.UpperAngle;

            return (angle >= 0 && angle <= this.Properties.UpperAngle) || (angle >= this.Properties.LowerAngle && angle <= 360);
        }

        public IExportJoint GetExportData(List<IEditorShape> bodies)
        {
            return new RevoluteJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(this.Body1),
                BodyIndex2 = bodies.IndexOf(this.Body2),
                R1 = this.r1,
                R2 = this.r2,
                CollideConnected = this.Properties.CollideConnected,
                LimitIsEnabled = this.Properties.LimitIsEnabled,
                LowerAngle = this.Properties.LowerAngle,
                UpperAngle = this.Properties.UpperAngle,
                Motor = this.Properties.Motor,
                MotorSpeed = this.Properties.MotorSpeed,
                MaxMotorTorque = this.Properties.MaxMotorTorque,
                SoftData = this.Properties.Soft.GetExportData(),
                BreakWhenMaxForceIsReached = this.Properties.BreakWhenMaxForceIsReached,
                MaxForceToBreak = this.Properties.MaxForceToBreak,
            };
        }
        public bool IsPointInside(Vec2D position)
        {
            if (EditorShapeHelper.IsPointAboveLine(this.Body1.Center, this.anchorWorldPosition1, position)) return true;
            if (EditorShapeHelper.IsPointAboveLine(this.Body2.Center, this.anchorWorldPosition1, position)) return true;

            return (this.anchorWorldPosition1 - position).Length() < 5;
        }


        public void DisableLimits()
        {
            this.Properties.LimitIsEnabled = false;
        }

        public void EnableLimits()
        {
            this.Properties.LimitIsEnabled = true;
        }

        public bool IsLimitDisabled()
        {
            return !this.Properties.LimitIsEnabled;
        }

        public bool IsPointAboveLimitArmA(Vec2D position)
        {
            if (this.Properties.LimitIsEnabled == false) return false;

            Vec2D a1ToPos = position - this.anchorWorldPosition1;

            if (this.limitArmA * a1ToPos < 0) return false;

            Vec2D pOnArmA = this.anchorWorldPosition1 + this.limitArmA * (this.limitArmA * a1ToPos);

            if ((pOnArmA - position).Length() > 10) return false;
            if ((pOnArmA - this.anchorWorldPosition1).Length() > this.limitArmLength) return false;

            return true;
        }

        public bool IsPointAboveLimitArmB(Vec2D position)
        {
            if (this.Properties.LimitIsEnabled == false) return false;

            Vec2D a1ToPos = position - this.anchorWorldPosition1;

            if (this.limitArmB * a1ToPos < 0) return false;

            Vec2D pOnArmB = this.anchorWorldPosition1 + this.limitArmB * (this.limitArmB * a1ToPos);

            if ((pOnArmB - position).Length() > 10) return false;
            if ((pOnArmB - this.anchorWorldPosition1).Length() > this.limitArmLength) return false;

            return true;
        }

        public float GetLimitAngle(Vec2D position)
        {
            Vec2D r1 = (this.Body1.Center - this.anchorWorldPosition1).Normalize();
            Vec2D dir = (position - this.anchorWorldPosition1).Normalize();
            return Vec2D.Angle360(r1, dir);
        }
    }
}
