using EditorControl.Model.EditorShape;
using EditorControl.Model.Function.Joints;
using EditorControl.ViewModel.Joints;
using GraphicPanels;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.EditorJoint
{
    internal class EditorDistanceJoint : IEditorJoint, IJointWithLinearMinMax
    {
        //Maximal erlaubter Wert für Propertys.MinLength und Propertys.MaxLength, wenn das Limit aktiviert ist 
        private const float MaxLimitToCalculateWith = 10000;

        private Vec2D anchor1;
        private Vec2D anchor2;
        private Vec2D centerToA1;
        private Vec2D a1ToTangent;

        private Vec2D r1;
        private Vec2D r2;

        public IEditorShape Body1 { get; }
        public IEditorShape Body2 { get; }
        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Blue;
        public Vec2D Center { get; private set; }
        public bool SupportsDefineLimit { get; } = true;

        public Pen MinLimitPen { get; set; } = Pens.Blue;
        public Pen MaxLimitPen { get; set; } = Pens.Blue;

        public DistanceJointPropertyViewModel Properties { get; }


        public bool DrawLimits { get; set; } = false;

        public bool LimitIsEnabled
        {
            get => this.Properties.LimitIsEnabled;
            set => this.Properties.LimitIsEnabled = value;
        }

        public float MinLength
        {
            get => this.Properties.MinLength;
            set => this.Properties.MinLength = value;
        }

        public float MaxLength
        {
            get => this.Properties.MaxLength;
            set => this.Properties.MaxLength = value;
        }


        //Neu anlegen
        public EditorDistanceJoint(IEditorShape body1, IEditorShape body2, Vec2D r1, Vec2D r2)
        {
            this.Body1 = body1;
            this.Body2 = body2;
            this.r1 = r1;
            this.r2 = r2;

            UpdateAfterMovingBodys();
            this.Properties = new DistanceJointPropertyViewModel();
        }

        //Aus Datei einladen
        public EditorDistanceJoint(DistanceJointExportData ctor, List<IEditorShape> shapes)
        {
            this.Body1 = shapes[ctor.BodyIndex1];
            this.Body2 = shapes[ctor.BodyIndex2];
            this.r1 = ctor.R1;
            this.r2 = ctor.R2;

            UpdateAfterMovingBodys();
            this.Properties = new DistanceJointPropertyViewModel()
            {
                CollideConnected = ctor.CollideConnected,
                LengthPosition = ctor.LengthPosition,
                LimitIsEnabled = ctor.LimitIsEnabled,
                MinLength = ctor.MinLength,
                MaxLength = ctor.MaxLength,
                Soft = new ViewModel.SoftPropertyViewModel(ctor.SoftData)
            };
        }

        public void UpdateAfterMovingBodys()
        {
            this.anchor1 = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body1, this.r1);
            this.anchor2 = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body2, this.r2);
            this.Center = anchor1 + (anchor2 - anchor1) / 2;
            this.centerToA1 = anchor1 - this.Center;
            this.a1ToTangent = Vec2D.CrossWithZ(this.centerToA1, 1).Normalize() * 10;
        }

        public void MoveTo(Vec2D position)
        {
            this.Center = position;
            UpdateAfterMovingBodys();
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
                float baseLength = (this.anchor2 - this.anchor1).Length();

                Vec2D dir = (this.anchor2 - this.anchor1).Normalize();

                if (this.Properties.MinLength >= 0 && this.Properties.MinLength < MaxLimitToCalculateWith)
                {
                    Vec2D min = this.anchor1 + dir * this.Properties.MinLength * baseLength;

                    float angle = Vec2D.Angle360YMirrored(new Vec2D(1, 0), dir);
                    panel.DrawCircleArc(this.MinLimitPen, min.ToGrx(), 15, angle + 90, angle - 90, false);

                    panel.DrawFillCircle(this.MinLimitPen.Color, min.ToGrx(), 3);
                    panel.DrawStringOnCircleBorder("Min=" + (int)(this.Properties.MinLength * 100), 20, Color.Black, min, dir.Spin90());
                }

                if (this.Properties.MaxLength >= 0 && this.Properties.MaxLength < MaxLimitToCalculateWith)
                {
                    Vec2D max = this.anchor1 + dir * this.Properties.MaxLength * baseLength;

                    float angle = Vec2D.Angle360YMirrored(new Vec2D(1, 0), dir);
                    panel.DrawCircleArc(this.MaxLimitPen, max.ToGrx(), 15, angle - 90, angle + 90, false);

                    panel.DrawFillCircle(this.MaxLimitPen.Color, max.ToGrx(), 3);
                    panel.DrawStringOnCircleBorder("Max=" + (int)(this.Properties.MaxLength * 100), 20, Color.Black, max, dir.Spin90());
                }
            }
        }

        private void Draw(GraphicPanel2D panel, Pen pen)
        {
            Vec2D a1 = this.Center - this.centerToA1;
            Vec2D a2 = this.Center + this.centerToA1;
            panel.DrawLine(pen, a1.ToGrx(), a2.ToGrx());
            panel.DrawLine(pen, (a1 - this.a1ToTangent).ToGrx(), (a1 + this.a1ToTangent).ToGrx());
            panel.DrawLine(pen, (a2 - this.a1ToTangent).ToGrx(), (a2 + this.a1ToTangent).ToGrx());
        }

        //Speichern
        public IExportJoint GetExportData(List<IEditorShape> shapes)
        {
            return new DistanceJointExportData()
            {
                BodyIndex1 = shapes.IndexOf(this.Body1),
                BodyIndex2 = shapes.IndexOf(this.Body2),
                R1 = this.r1,
                R2 = this.r2,
                CollideConnected = this.Properties.CollideConnected,
                LengthPosition = this.Properties.LengthPosition,
                LimitIsEnabled = this.Properties.LimitIsEnabled,
                MinLength = this.Properties.MinLength,
                MaxLength = this.Properties.MaxLength,
                SoftData = this.Properties.Soft.GetExportData()
            };
        }
        public bool IsPointInside(Vec2D position)
        {
            float l = this.centerToA1.Length();
            Vec2D toA1 = this.centerToA1 / l;
            float projection = (position - this.Center) * toA1;
            if (Math.Abs(projection) > l) return false;

            Vec2D p = this.Center + toA1 * projection;
            return (p - position).Length() < 3;
        }

        public bool IsPointAboveMinLimit(Vec2D position)
        {
            if (this.Properties.MinLength >= 0 && this.Properties.MinLength < MaxLimitToCalculateWith && this.Properties.LimitIsEnabled)
            {
                float baseLength = (this.anchor2 - this.anchor1).Length();
                Vec2D dir = (this.anchor2 - this.anchor1).Normalize();
                Vec2D min = this.anchor1 + dir * this.Properties.MinLength * baseLength;

                return (position - min).Length() < 10;
            }

            return false;
        }

        public bool IsPointAboveMaxLimit(Vec2D position)
        {
            if (this.Properties.MaxLength >= 0 && this.Properties.MaxLength < MaxLimitToCalculateWith && this.Properties.LimitIsEnabled)
            {
                float baseLength = (this.anchor2 - this.anchor1).Length();
                Vec2D dir = (this.anchor2 - this.anchor1).Normalize();
                Vec2D max = this.anchor1 + dir * this.Properties.MaxLength * baseLength;

                return (position - max).Length() < 10;
            }

            return false;
        }

        public float GetMinMaxDistance(Vec2D position)
        {
            Vec2D dir = (this.anchor2 - this.anchor1).Normalize();
            Vec2D a1ToPos = position - this.anchor1;

            if (dir * a1ToPos < 0) return 0;

            float baseLength = (this.anchor2 - this.anchor1).Length();
            return Vec2D.Projection(position - this.anchor1, dir).Length() / baseLength;
        }
    }
}
