using LevelEditorGlobal;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System;
using System.Drawing;
using PhysicGlobal;
using LevelEditorExports.Simulator;

namespace PhysicItemEditorControl.Model.MouseClickable
{
    //Diese Klasse hilft dabei, das man mit der Maus einzelne Körper von einer PhysicScene anklicken kann und deren 
    //CollisionCategory und TagData festlegen kann
    internal class MouseClickableBody : ICollidable, IMouseclickableWithTagData
    {
        private IExportRigidBody exportBody; //Enthält die CollisionCategory
        private IPublicRigidBody runtimBody; //Läßt sich leichter zeichnen
        private PhysicGlobal.BoundingBox sceneBoundingBox;

        public MouseClickableBody(IExportRigidBody exportBody, IPublicRigidBody runtimBody, PhysicGlobal.BoundingBox sceneBoundingBox, int bodyIndex)
        {
            this.exportBody = exportBody;
            this.sceneBoundingBox = sceneBoundingBox;
            this.runtimBody = runtimBody;
            Id = bodyIndex;
        }

        public int CollisionCategory { get => exportBody.CollisionCategory; set => exportBody.CollisionCategory = value; } //ICollidable
        public int Id { get; } //ITagable
        public TagType TypeName { get => TagType.Body; } //ITagable

        public void Draw(IDrawingPanel panel)
        {
            //Mache nichts, da bereits der PhysicSceneDrawer das Objekt zeichnet
        }

        public void DrawBorder(IDrawingPanel panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-sceneBoundingBox.X, -sceneBoundingBox.Y, 0));
            DrawBody(panel, borderPen);

            panel.PopMatrix();
        }

        private void DrawBody(IDrawingPanel panel, Pen borderPen)
        {
            if (runtimBody is IPublicRigidRectangle)
                DrawRectangle((IPublicRigidRectangle)runtimBody, panel, borderPen);

            if (runtimBody is IPublicRigidPolygon)
                DrawPolygon((IPublicRigidPolygon)runtimBody, panel, borderPen);

            if (runtimBody is IPublicRigidCircle)
                DrawCircle((IPublicRigidCircle)runtimBody, panel, borderPen);
        }

        private void DrawRectangle(IPublicRigidRectangle rec, IDrawingPanel panel, Pen borderPen)
        {
            panel.DrawPolygon(borderPen, rec.Vertex);
        }

        private void DrawPolygon(IPublicRigidPolygon polygon, IDrawingPanel panel, Pen borderPen)
        {
            panel.DrawPolygon(borderPen, polygon.Vertex);
        }

        private void DrawCircle(IPublicRigidCircle circle, IDrawingPanel panel, Pen borderPen)
        {
            panel.DrawCircle(borderPen, circle.Center, circle.Radius);
        }

        public bool IsPointInside(Vec2D point, Matrix4x4 screenToLocal)
        {
            screenToLocal *= Matrix4x4.Translate(sceneBoundingBox.X, sceneBoundingBox.Y, 0);
            point = Matrix4x4.MultPosition(screenToLocal, point);

            if (runtimBody is IPublicRigidRectangle)
                return PhysicGlobal.PolygonHelper.PointIsInsidePolygon((runtimBody as IPublicRigidRectangle).Vertex, point);

            if (runtimBody is IPublicRigidPolygon)
                return PhysicGlobal.PolygonHelper.PointIsInsidePolygon((runtimBody as IPublicRigidPolygon).Vertex, point);

            if (runtimBody is IPublicRigidCircle)
                return IsPointInsideCircle((IPublicRigidCircle)runtimBody, point);

            throw new Exception("Unknown type " + runtimBody.GetType());
        }

        public Matrix4x4 GetScreenToLocalMatrix()
        {
            float angleInDegree = runtimBody.Angle * 180 / (float)Math.PI;
            return Matrix4x4.Translate(sceneBoundingBox.X - runtimBody.Center.X, sceneBoundingBox.Y - runtimBody.Center.Y, 0) * Matrix4x4.Rotate(-angleInDegree, 0, 0, 1);
        }

        private static bool IsPointInsideCircle(IPublicRigidCircle circle, Vec2D p)
        {
            return (p - circle.Center).Length() < circle.Radius;
        }

        public float GetArea()
        {
            if (runtimBody is IPublicRigidRectangle)
                return GetRectangleArea((IPublicRigidRectangle)runtimBody);

            if (runtimBody is IPublicRigidPolygon)
                return GetPolygonArea((IPublicRigidPolygon)runtimBody);

            if (runtimBody is IPublicRigidCircle)
                return GetCircleArea((IPublicRigidCircle)runtimBody);

            throw new Exception("Unknown type " + runtimBody.GetType());
        }

        private float GetRectangleArea(IPublicRigidRectangle rectangle)
        {
            return rectangle.Size.X * rectangle.Size.Y;
        }

        private float GetPolygonArea(IPublicRigidPolygon polygon)
        {
            return PolygonHelper.GetAreaFromPolygon(polygon.Vertex);
        }

        private float GetCircleArea(IPublicRigidCircle circle)
        {
            return (float)Math.PI * circle.Radius * circle.Radius;
        }
    }
}
