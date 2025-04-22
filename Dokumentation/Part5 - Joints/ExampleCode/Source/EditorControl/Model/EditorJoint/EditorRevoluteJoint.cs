using EditorControl.Model.EditorShape;
using EditorControl.ViewModel.Joints;
using GraphicPanels;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorJoint
{
    internal class EditorRevoluteJoint : IEditorJoint
    {
        private float circleForP1 = 10; //Diesen Abstand haben die P1-Punkte zum Center
        private float limitArmLength = 80; //Diesen Abstand haben die P1-Punkte zum Center

        private Vec2D leverArmAP1, leverArmBP1;//Arm von Center Body1.Center/Body2.Center
        private Vec2D limitArmA, limitArmB;    //Richtungsvektor

        public IEditorShape Body1 { get; }
        public IEditorShape Body2 { get; }
        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Blue;
        public Vec2D Center { get; private set; }
        public bool SupportsDefineLimit { get; } = true;

        public RevoluteJointPropertyViewModel Properties { get; }

        public bool DrawLimits = false;
        public Pen LimitArmAPen { get; set; } = Pens.Blue;
        public Pen LimitArmBPen { get; set; } = Pens.Blue;

        //Neu anlegen
        public EditorRevoluteJoint(IEditorShape body1, IEditorShape body2, Vec2D anchorWorldPosition)
        {
            this.Body1 = body1;
            this.Body2 = body2;

            this.Center = anchorWorldPosition;
            

            this.Properties = new RevoluteJointPropertyViewModel();
            UpdateAfterMovingBodys();            
        }

        //Aus Datei einladen
        public EditorRevoluteJoint(RevoluteJointExportData ctor, List<IEditorShape> shapes)
        {
            this.Body1 = shapes[ctor.BodyIndex1];
            this.Body2 = shapes[ctor.BodyIndex2];
            this.Center = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body1, ctor.R1);
            
            this.Properties = new RevoluteJointPropertyViewModel()
            {
                CollideConnected = ctor.CollideConnected,
                LimitIsEnabled = ctor.LimitIsEnabled,
                LowerAngle = ctor.LowerAngle,
                UpperAngle = ctor.UpperAngle,
                Motor = ctor.Motor,
                MotorSpeed = ctor.MotorSpeed,
                MotorPosition = ctor.MotorPosition,
                MaxMotorTorque = ctor.MaxMotorTorque,
                Soft = new ViewModel.SoftPropertyViewModel(ctor.SoftData)
            };
            UpdateAfterMovingBodys();
        }

        public void MoveTo(Vec2D position)
        {
            this.Center = position;
            UpdateAfterMovingBodys();
        }
        public void UpdateAfterMovingBodys()
        {
            Vec2D r1 = (this.Body1.Center - this.Center).Normalize();
            Vec2D r2 = (this.Body2.Center - this.Center).Normalize();

            this.leverArmAP1 = this.Center + r1 * this.circleForP1;
            this.leverArmBP1 = this.Center + r2 * this.circleForP1;

            this.limitArmA = Vec2D.GetV2FromAngle360(r1, this.Properties.LowerAngle);
            this.limitArmB = Vec2D.GetV2FromAngle360(r1, this.Properties.UpperAngle);

            
        }

        

        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            if (this.Backcolor != Color.Transparent)
            {
                panel.DrawFillCircle(this.Backcolor, this.Center.ToGrx(), 5);
                panel.DrawLine(new Pen(this.Backcolor, 3), this.leverArmAP1.ToGrx(), this.Body1.Center.ToGrx());
                panel.DrawLine(new Pen(this.Backcolor, 3), this.leverArmBP1.ToGrx(), this.Body2.Center.ToGrx());
            }

            panel.DrawLine(this.BorderPen, this.leverArmAP1.ToGrx(), this.Body1.Center.ToGrx());
            panel.DrawLine(this.BorderPen, this.leverArmBP1.ToGrx(), this.Body2.Center.ToGrx());
            panel.DrawCircle(this.BorderPen, this.Center.ToGrx(), this.circleForP1);

            //Testausgabe
            //if (IsAngleInMinMaxRange(GetLimitAngle(this.Body2.Center)))
            //    panel.DrawString(Center.ToGrx(), Color.Green, 30, "JA");
            //else
            //    panel.DrawString(Center.ToGrx(), Color.Red, 30, "NEIN");

            

            panel.DrawFillCircle(this.BorderPen.Color, this.Center.ToGrx(), 2);

            if (this.DrawLimits && this.Properties.LimitIsEnabled)
            {
                Vec2D limitAP1 = this.Center + this.limitArmA * this.circleForP1;
                Vec2D limitAP2 = this.Center + this.limitArmA * this.limitArmLength;
                panel.DrawLine(this.LimitArmAPen, limitAP1.ToGrx(), limitAP2.ToGrx());

                Vec2D limitBP1 = this.Center + this.limitArmB * this.circleForP1;
                Vec2D limitBP2 = this.Center + this.limitArmB * this.limitArmLength;
                panel.DrawLine(this.LimitArmBPen, limitBP1.ToGrx(), limitBP2.ToGrx());

                float lowerAngle = Vec2D.Angle360YMirrored(new Vec2D(1, 0), this.limitArmA);
                float upperAngle = Vec2D.Angle360YMirrored(new Vec2D(1, 0), this.limitArmB);
                panel.DrawCircleArc(new Pen(Color.Yellow, 5), this.Center.ToGrx(), (int)this.limitArmLength, upperAngle, lowerAngle, false);

                panel.DrawStringOnCircleBorder("1", 20, Color.Green, this.Body1.Center, (this.Body1.Center - this.Center).Normalize());
                panel.DrawStringOnCircleBorder("2", 20, Color.Green, this.Body2.Center, (this.Body2.Center - this.Center).Normalize());
                panel.DrawStringOnCircleBorder("Min", 20, Color.Black, limitAP2, limitArmA.Normalize());
                panel.DrawStringOnCircleBorder("Max", 20, Color.Black, limitBP2, limitArmB.Normalize());

                //Testausgabe
                //float b2 = GetLimitAngle(this.Body2.Center);
                //DrawStringOnCircleBorder(panel, "Pos " + (int)b2, 20, Color.Green, Center + (this.Body2.Center - Center).Normalize() * limitArmLength, (this.Body2.Center - this.Center).Normalize());
                //DrawStringOnCircleBorder(panel, "Min " + (int)this.Properties.LowerAngle, 20, Color.Black, limitAP2, limitArmA.Normalize());
                //DrawStringOnCircleBorder(panel, "Max " + (int)this.Properties.UpperAngle, 20, Color.Black, limitBP2, limitArmB.Normalize());

            }
        }

        

        private bool IsAngleInMinMaxRange(float angle)
        {
            if (this.Properties.LowerAngle < this.Properties.UpperAngle)
                return angle >= this.Properties.LowerAngle && angle <= this.Properties.UpperAngle;



            return (angle >= 0 && angle <= this.Properties.UpperAngle) || (angle >= this.Properties.LowerAngle && angle <=360);
        }

        public IExportJoint GetExportData(List<IEditorShape> bodies)
        {
            return new RevoluteJointExportData()
            {
                BodyIndex1 = bodies.IndexOf(this.Body1),
                BodyIndex2 = bodies.IndexOf(this.Body2),
                R1 = EditorShapeHelper.GetLocalBodyDirection(this.Body1, this.Center),
                R2 = EditorShapeHelper.GetLocalBodyDirection(this.Body2, this.Center),
                CollideConnected = this.Properties.CollideConnected,
                LimitIsEnabled = this.Properties.LimitIsEnabled,
                LowerAngle = this.Properties.LowerAngle,
                UpperAngle = this.Properties.UpperAngle,
                Motor = this.Properties.Motor,
                MotorSpeed = this.Properties.MotorSpeed,
                MotorPosition = this.Properties.MotorPosition,
                MaxMotorTorque = this.Properties.MaxMotorTorque,
                SoftData = this.Properties.Soft.GetExportData()
            };
        }
        public bool IsPointInside(Vec2D position)
        {
            if (EditorShapeHelper.IsPointAboveLine(this.Body1.Center, this.Center, position)) return true;
            if (EditorShapeHelper.IsPointAboveLine(this.Body2.Center, this.Center, position)) return true;

            return (this.Center - position).Length() < 5;
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

            Vec2D a1ToPos = position - this.Center;

            if (this.limitArmA * a1ToPos < 0) return false;

            Vec2D pOnArmA = this.Center + this.limitArmA * (this.limitArmA * a1ToPos);

            if ((pOnArmA - position).Length() > 10) return false;
            if ((pOnArmA - this.Center).Length() > this.limitArmLength) return false;

            return true;
        }

        public bool IsPointAboveLimitArmB(Vec2D position)
        {
            if (this.Properties.LimitIsEnabled == false) return false;

            Vec2D a1ToPos = position - this.Center;

            if (this.limitArmB * a1ToPos < 0) return false;

            Vec2D pOnArmB = this.Center + this.limitArmB * (this.limitArmB * a1ToPos);

            if ((pOnArmB - position).Length() > 10) return false;
            if ((pOnArmB - this.Center).Length() > this.limitArmLength) return false;

            return true;
        }

        public float GetLimitAngle(Vec2D position)
        {
            Vec2D r1 = (this.Body1.Center - this.Center).Normalize();
            Vec2D dir = (position - this.Center).Normalize();
            return Vec2D.Angle360(r1, dir);
        }
    }
}
