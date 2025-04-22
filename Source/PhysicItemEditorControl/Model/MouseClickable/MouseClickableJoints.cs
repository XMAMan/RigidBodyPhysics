using GraphicMinimal;
using GraphicPanels;
using LevelEditorGlobal;
using RigidBodyPhysics.RuntimeObjects.Joints;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static LevelEditorGlobal.ITagable;

namespace PhysicItemEditorControl.Model.MouseClickable
{
    internal class MouseClickableJoints : IMouseclickableWithTagData
    {
        private IPublicJoint runtimJoint; //Läßt sich leichter zeichnen
        private RectangleF sceneBoundingBox;

        public MouseClickableJoints(IPublicJoint runtimJoint, RectangleF sceneBoundingBox, int bodyIndex)
        {
            this.sceneBoundingBox = sceneBoundingBox;
            this.runtimJoint = runtimJoint;
            Id = bodyIndex;
        }

        public int Id { get; } //ITagable
        public TagType TypeName { get => TagType.Joint; } //ITagable

        public void Draw(GraphicPanel2D panel)
        {
            DrawBorder(panel, Pens.Blue);
        }

        public void DrawBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-sceneBoundingBox.X, -sceneBoundingBox.Y, 0));
            DrawJoint(panel, borderPen);

            panel.PopMatrix();
        }

        private void DrawJoint(GraphicPanel2D panel, Pen borderPen)
        {
            if (runtimJoint is IPublicDistanceJoint)
                DrawDistanceJoint((IPublicDistanceJoint)runtimJoint, panel, borderPen);
           
            if (runtimJoint is IPublicPrismaticJoint)
                DrawPrismaticJoint((IPublicPrismaticJoint)runtimJoint, panel, borderPen);

            if (runtimJoint is IPublicRevoluteJoint)
                DrawRevoluteJoint((IPublicRevoluteJoint)runtimJoint, panel, borderPen);

            if (runtimJoint is IPublicWeldJoint)
                DrawWeldJoint((IPublicWeldJoint)runtimJoint, panel, borderPen);

            if (runtimJoint is IPublicWheelJoint)
                DrawWheelJoint((IPublicWheelJoint)runtimJoint, panel, borderPen);
        }

        private void DrawDistanceJoint(IPublicDistanceJoint j, GraphicPanel2D panel, Pen borderPen)
        {
            panel.DrawLine(borderPen, j.Anchor1.ToGrx(), j.Anchor2.ToGrx());
        }

        private void DrawPrismaticJoint(IPublicPrismaticJoint j, GraphicPanel2D panel, Pen borderPen)
        {
            var p1 = j.Anchor1.ToGrx();
            var p2 = j.Anchor2.ToGrx();
            panel.DrawLine(borderPen, p1, p2);

            float d = (p2 - p1).Length();
            float tangentLength = Math.Min(d * 0.1f, 10);

            var tangent = ((p2 - p1) / d).Spin90() * tangentLength;
            panel.DrawLine(borderPen, p1 +tangent, p2 + tangent);
            panel.DrawLine(borderPen, p1 - tangent, p2 - tangent);
            panel.DrawLine(borderPen, p1 + tangent, p1 - tangent);
        }

        private void DrawRevoluteJoint(IPublicRevoluteJoint j, GraphicPanel2D panel, Pen borderPen)
        {
            var p1 = j.Anchor1.ToGrx();
            var b1 = j.Body1.Center.ToGrx();
            var b2 = j.Body2.Center.ToGrx();

            var r1 = b1 - p1;
            var r2 = b2 - p1;

            float r1Length = Math.Max(0.0001f, r1.Length());
            float r2Length = Math.Max(0.0001f, r2.Length());

            r1 /= r1Length;
            r2 /= r2Length;


            float armLength = Math.Min(r1Length, r2Length);
            float circleRadius = armLength / 8;

            if (circleRadius > 1)
                panel.DrawCircle(borderPen, p1, circleRadius); 
            else
                panel.DrawCircleWithLines(borderPen, p1, circleRadius, 10);

            panel.DrawLine(borderPen, p1 + r1 * circleRadius, p1 + r1 * armLength);
            panel.DrawLine(borderPen, p1 + r2 * circleRadius, p1 + r2 * armLength);
        }

        private void DrawWeldJoint(IPublicWeldJoint j, GraphicPanel2D panel, Pen borderPen)
        {
            var p1 = j.Anchor1.ToGrx();
            var b1 = j.Body1.Center.ToGrx();
            var b2 = j.Body2.Center.ToGrx();
            var r1 = b1 - p1;
            var r2 = b2 - p1;

            float r1Length = Math.Max(0.0001f, r1.Length());
            float r2Length = Math.Max(0.0001f, r2.Length());
            float armLength = Math.Min(r1Length, r2Length);
            float circleRadius = armLength / 5;

            int cornerCount = 7;
            float radius = circleRadius;

            List<Vector2D> points = new List<Vector2D>();
            for (int i = 0; i < cornerCount; i++)
            {
                points.Add(p1 + new Vector2D((float)Math.Cos(i / (float)cornerCount * 2 * Math.PI), (float)Math.Sin(i / (float)cornerCount * 2 * Math.PI)) * radius);

                panel.DrawLine(borderPen, p1, points.Last());
            }

            panel.DrawPolygon(borderPen, points);
        }

        private void DrawWheelJoint(IPublicWheelJoint j, GraphicPanel2D panel, Pen borderPen)
        {
            var p1 = j.Anchor1.ToGrx();
            var p2 = j.Anchor2.ToGrx();
            panel.DrawLine(borderPen, p1, p2);

            float d = (p2 - p1).Length();
            float radius = Math.Min(d * 0.1f, 15);

            panel.DrawCircle(borderPen, p2, radius);
        }

        public bool IsPointInside(Vector2D point, Matrix4x4 screenToLocal)
        {
            screenToLocal *= Matrix4x4.Translate(sceneBoundingBox.X, sceneBoundingBox.Y, 0);
            point = Matrix4x4.MultPosition(screenToLocal, new Vector3D(point.X, point.Y, 0)).XY;

            if (runtimJoint is IPublicDistanceJoint)
                return IsPointAboveDistanceJoint((IPublicDistanceJoint)runtimJoint, point);

            if (runtimJoint is IPublicPrismaticJoint)
                return IsPointAbovePrismaticJoint((IPublicPrismaticJoint)runtimJoint, point);

            if (runtimJoint is IPublicRevoluteJoint)
                return IsPointAboveRevoluteJoint((IPublicRevoluteJoint)runtimJoint, point);

            if (runtimJoint is IPublicWeldJoint)
                return IsPointAboveWeldJoint((IPublicWeldJoint)runtimJoint, point);

            if (runtimJoint is IPublicWheelJoint)
                return IsPointAboveWheelJoint((IPublicWheelJoint)runtimJoint, point);

            throw new Exception("Unknown type " + runtimJoint.GetType());
        }

        private bool IsPointAboveDistanceJoint(IPublicDistanceJoint j, Vector2D point)
        {
            return MathHelper.IsPointAboveLine(j.Anchor1.ToGrx(), j.Anchor2.ToGrx(), point);
        }

        private bool IsPointAbovePrismaticJoint(IPublicPrismaticJoint j, Vector2D point)
        {
            return MathHelper.IsPointAboveLine(j.Anchor1.ToGrx(), j.Anchor2.ToGrx(), point);
        }

        private bool IsPointAboveRevoluteJoint(IPublicRevoluteJoint j, Vector2D point)
        {
            return (j.Anchor1.ToGrx() - point).Length() < 10;
        }

        private bool IsPointAboveWeldJoint(IPublicWeldJoint j, Vector2D point)
        {
            return (j.Anchor1.ToGrx() - point).Length() < 20;
        }

        private bool IsPointAboveWheelJoint(IPublicWheelJoint j, Vector2D point)
        {
            return MathHelper.IsPointAboveLine(j.Anchor1.ToGrx(), j.Anchor2.ToGrx(), point);
        }

        

        public Matrix4x4 GetScreenToLocalMatrix()
        {
            return Matrix4x4.Ident();
        }

        public float GetArea()
        {
            return 1;
        }
    }
}
