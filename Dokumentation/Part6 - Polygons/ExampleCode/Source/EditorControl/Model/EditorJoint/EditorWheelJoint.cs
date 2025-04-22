using EditorControl.Model.EditorShape;
using EditorControl.Model.Function.Joints;
using EditorControl.ViewModel.Joints;
using GraphicPanels;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorJoint
{
    internal class EditorWheelJoint : IEditorJoint, IJointWithLinearMinMax
    {
        //Maximal erlaubter Wert für Propertys.MinLength und Propertys.MaxLength, wenn das Limit aktiviert ist 
        private const float MaxLimitToCalculateWith = 10000;

        private Vec2D anchor1;
        private Vec2D anchor2;

        private Vec2D r1Dir;
        private Vec2D d;         //Zeigt von Body1.Center nach anchor2
        private Vec2D r1Point;   //anchor2 auf der r1Dir-Linie
        private Vec2D tangent;   //Tangente zu r1Dir
        private float baseLength;//Abstand zwischen r1Point und Body1.Center

        private Vec2D r1;
        private Vec2D r2;

        public IEditorShape Body1 { get; }
        public IEditorShape Body2 { get; }
        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Blue;
        public bool SupportsDefineLimit { get; } = true;

        public Pen MinLimitPen { get; set; } = Pens.Blue;
        public Pen MaxLimitPen { get; set; } = Pens.Blue;

        public WheelJointPropertyViewModel Properties { get; }


        public bool DrawLimits { get; set; } = false;

        public bool LimitIsEnabled
        {
            get => this.Properties.LimitIsEnabled;
            set => this.Properties.LimitIsEnabled = value;
        }

        public float MinLength
        {
            get => this.Properties.MinTranslation;
            set => this.Properties.MinTranslation = value;
        }

        public float MaxLength
        {
            get => this.Properties.MaxTranslation;
            set => this.Properties.MaxTranslation = value;
        }

        //Neu anlegen
        public EditorWheelJoint(IEditorShape body1, IEditorShape body2, Vec2D r1, Vec2D r2)
        {
            this.Body1 = body1;
            this.Body2 = body2;
            this.r1 = r1;
            this.r2 = r2;

            UpdateAfterMovingBodys();
            this.Properties = new WheelJointPropertyViewModel();
        }

        //Aus Datei einladen
        public EditorWheelJoint(WheelJointExportData ctor, List<IEditorShape> shapes)
        {
            this.Body1 = shapes[ctor.BodyIndex1];
            this.Body2 = shapes[ctor.BodyIndex2];
            this.r1 = ctor.R1;
            this.r2 = ctor.R2;

            UpdateAfterMovingBodys();
            this.Properties = new WheelJointPropertyViewModel()
            {
                CollideConnected = ctor.CollideConnected,
                LimitIsEnabled = ctor.LimitIsEnabled,
                MinTranslation = ctor.MinTranslation,
                MaxTranslation = ctor.MaxTranslation,
                Motor = ctor.Motor,
                MotorSpeed = ctor.MotorSpeed,
                MaxMotorForce = ctor.MaxMotorForce,
                Soft = new ViewModel.SoftPropertyViewModel(ctor.SoftData),
                BreakWhenMaxForceIsReached = ctor.BreakWhenMaxForceIsReached,
                MaxForceToBreak = ctor.MaxForceToBreak,
            };
        }

        public void UpdateAfterMovingBodys()
        {
            this.anchor1 = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body1, this.r1);
            this.anchor2 = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body2, this.r2);

            this.r1Dir = (this.anchor1 - this.Body1.Center).Normalize();
            this.d = this.anchor2 - this.Body1.Center;
            this.baseLength = r1.Length();
            this.r1Point = this.Body1.Center + r1Dir * baseLength;
            this.tangent = Vec2D.CrossWithZ(r1Dir, 1) * 10;
        }

        public void Draw(GraphicPanel2D panel) //Zeichnet die Editor-Daten
        {
            if (this.Backcolor != Color.Transparent)
            {
                Draw(panel, new Pen(this.Backcolor, 4));
            }

            Draw(panel, this.BorderPen);

            if (this.DrawLimits && this.Properties.LimitIsEnabled)
            {
                if (this.Properties.MinTranslation >= -MaxLimitToCalculateWith && this.Properties.MinTranslation < MaxLimitToCalculateWith)
                {
                    Vec2D min = this.Body1.Center + this.r1Dir * this.Properties.MinTranslation * this.baseLength;

                    float angle = Vec2D.Angle360YMirrored(new Vec2D(1, 0), this.r1Dir);
                    panel.DrawCircleArc(this.MinLimitPen, min.ToGrx(), 15, angle + 90, angle - 90, false);

                    panel.DrawFillCircle(this.MinLimitPen.Color, min.ToGrx(), 3);
                    panel.DrawStringOnCircleBorder("Min=" + (int)(this.Properties.MinTranslation * 100), 20, Color.Black, min, this.r1Dir.Spin90());
                }

                if (this.Properties.MaxTranslation >= -MaxLimitToCalculateWith && this.Properties.MaxTranslation < MaxLimitToCalculateWith)
                {
                    Vec2D max = this.Body1.Center + this.r1Dir * this.Properties.MaxTranslation * this.baseLength;

                    float angle = Vec2D.Angle360YMirrored(new Vec2D(1, 0), this.r1Dir);
                    panel.DrawCircleArc(this.MaxLimitPen, max.ToGrx(), 15, angle - 90, angle + 90, false);

                    panel.DrawFillCircle(this.MaxLimitPen.Color, max.ToGrx(), 3);
                    panel.DrawStringOnCircleBorder("Max=" + (int)(this.Properties.MaxTranslation * 100), 20, Color.Black, max, this.r1Dir.Spin90());
                }
            }
        }

        

        private void Draw(GraphicPanel2D panel, Pen pen)
        {
            panel.DrawLine(pen, this.Body1.Center.ToGrx(), r1Point.ToGrx());
            panel.DrawLine(pen, r1Point.ToGrx(), this.anchor2.ToGrx());            
            panel.DrawLine(pen, (this.Body1.Center - tangent).ToGrx(), (this.Body1.Center + tangent).ToGrx());
            panel.DrawCircle(pen, this.anchor2.ToGrx(), 15);
        }

        //Speichern
        public IExportJoint GetExportData(List<IEditorShape> shapes)
        {
            return new WheelJointExportData()
            {
                BodyIndex1 = shapes.IndexOf(this.Body1),
                BodyIndex2 = shapes.IndexOf(this.Body2),
                R1 = this.r1,
                R2 = this.r2,
                CollideConnected = this.Properties.CollideConnected,
                LimitIsEnabled = this.Properties.LimitIsEnabled,
                MinTranslation = this.Properties.MinTranslation,
                MaxTranslation = this.Properties.MaxTranslation,
                Motor = this.Properties.Motor,
                MotorSpeed = this.Properties.MotorSpeed,
                MaxMotorForce = this.Properties.MaxMotorForce,
                SoftData = this.Properties.Soft.GetExportData(),
                BreakWhenMaxForceIsReached = this.Properties.BreakWhenMaxForceIsReached,
                MaxForceToBreak = this.Properties.MaxForceToBreak,
            };
        }
        public bool IsPointInside(Vec2D position)
        {
            if (EditorShapeHelper.IsPointAboveLine(this.Body1.Center, this.r1Point, position)) return true;
            if (EditorShapeHelper.IsPointAboveLine(this.anchor2, this.r1Point, position)) return true;

            return false;
        }

        

        public bool IsPointAboveMinLimit(Vec2D position)
        {
            if (this.Properties.MinTranslation >= -MaxLimitToCalculateWith && this.Properties.MinTranslation < MaxLimitToCalculateWith && this.Properties.LimitIsEnabled)
            {
                Vec2D min = this.Body1.Center + this.r1Dir * this.Properties.MinTranslation * this.baseLength;

                return (position - min).Length() < 10;
            }

            return false;
        }

        public bool IsPointAboveMaxLimit(Vec2D position)
        {
            if (this.Properties.MaxTranslation >= -MaxLimitToCalculateWith && this.Properties.MaxTranslation < MaxLimitToCalculateWith && this.Properties.LimitIsEnabled)
            {
                Vec2D max = this.Body1.Center + this.r1Dir * this.Properties.MaxTranslation * this.baseLength;

                return (position - max).Length() < 10;
            }

            return false;
        }

        public float GetMinMaxDistance(Vec2D position)
        {
            return ((position - this.Body1.Center) * this.r1Dir) / baseLength;
        }
    }
}
