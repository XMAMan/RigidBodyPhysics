using GraphicMinimal;
using GraphicPanels;
using Part4.Model.Editor.EditorShape;
using Part4.ViewModel.Editor;
using PhysicEngine.ExportData.Joints;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Part4.Model.Editor.EditorJoint
{
    internal class EditorDistanceJoint : IEditorJoint
    {
        private Vector2D anchor1;
        private Vector2D anchor2;
        private Vector2D centerToA1;
        private Vector2D a1ToTangent;

        private Vector2D r1;
        private Vector2D r2;    

        public IEditorShape Body1 { get; }
        public IEditorShape Body2 { get; }
        public Color Backcolor { get; set; } = Color.Transparent;
        public Pen BorderPen { get; set; } = Pens.Blue;
        public Vector2D Center { get; private set; }

        public DistanceJointPropertyViewModel Properties { get; }

        
        //Neu anlegen
        public EditorDistanceJoint(IEditorShape body1, IEditorShape body2, Vector2D r1, Vector2D r2)
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
                LengthFactor = ctor.LengthFactor,
                MinLength = ctor.MinLength,
                MaxLength = ctor.MaxLength,
                SpringParameter = ctor.ParameterType,
                FrequencyHertz = ctor.FrequencyHertz,
                DampingRatio = ctor.DampingRatio,
                Stiffness = ctor.Stiffness,
                Damping = ctor.Damping,
            };
        }

        public void UpdateAfterMovingBodys()
        {
            this.anchor1 = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body1, this.r1);
            this.anchor2 = EditorShapeHelper.LocalBodyDirectionToWorldPosition(this.Body2, this.r2);
            this.Center = anchor1 + (anchor2 - anchor1) / 2;
            this.centerToA1 = anchor1 - this.Center;
            this.a1ToTangent = Vector2D.CrossWithZ(this.centerToA1, 1).Normalize() * 10;
        }

        public void MoveTo(Vector2D position)
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
        }

        private void Draw(GraphicPanel2D panel, Pen pen)
        {
            Vector2D a1 = this.Center - this.centerToA1;
            Vector2D a2 = this.Center + this.centerToA1;
            panel.DrawLine(pen, a1, a2);
            panel.DrawLine(pen, a1 - this.a1ToTangent, a1 + this.a1ToTangent);
            panel.DrawLine(pen, a2 - this.a1ToTangent, a2 + this.a1ToTangent);
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
                LengthFactor = this.Properties.LengthFactor,
                MinLength = this.Properties.MinLength,
                MaxLength = this.Properties.MaxLength,
                ParameterType = this.Properties.SpringParameter,
                FrequencyHertz = this.Properties.FrequencyHertz,
                DampingRatio = this.Properties.DampingRatio,
                Stiffness = this.Properties.Stiffness,
                Damping = this.Properties.Damping
            };
        }
        public bool IsPointInside(Vector2D position)
        {
            float l = this.centerToA1.Length();
            Vector2D toA1 = this.centerToA1 / l;
            float projection = (position - this.Center) * toA1;
            if (Math.Abs(projection) > l) return false;

            Vector2D p = this.Center + toA1 * projection;
            return (p - position).Length() < 3;
        }
    }
}
