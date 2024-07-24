using GraphicMinimal;
using GraphicPanels;
using LevelEditorGlobal;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using System.Drawing;
using static LevelEditorGlobal.ITagable;

namespace PhysicItemEditorControl.Model.MouseClickable
{
    internal class MouseClickableRotaryMotor : IMouseclickableWithTagData
    {
        private IPublicRotaryMotor runtimMotor; //Läßt sich leichter zeichnen
        private RectangleF sceneBoundingBox;

        public MouseClickableRotaryMotor(IPublicRotaryMotor runtimMotor, RectangleF sceneBoundingBox, int bodyIndex)
        {
            this.sceneBoundingBox = sceneBoundingBox;
            this.runtimMotor = runtimMotor;
            Id = bodyIndex;
        }

        public int Id { get; } //ITagable
        public TagType TypeName { get => TagType.Motor; } //ITagable

        public void Draw(GraphicPanel2D panel)
        {
            DrawBorder(panel, Pens.Blue);
        }
        public void DrawBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-sceneBoundingBox.X, -sceneBoundingBox.Y, 0));
            DrawCircle(borderPen, panel);

            panel.PopMatrix();
        }

        private void DrawCircle(Pen pen, GraphicPanel2D panel)
        {
            panel.DrawCircleArc(pen, this.runtimMotor.Body.Center.ToGrx(), 20, 30, 320, false);
            var p = Vec2D.RotatePointAroundPivotPoint(this.runtimMotor.Body.Center, this.runtimMotor.Body.Center + new Vec2D(20, 0), 320);
            var dir1 = Vec2D.RotatePointAroundPivotPoint(this.runtimMotor.Body.Center, this.runtimMotor.Body.Center + new Vec2D(20 + 10, 0 - 10), 320);
            var dir2 = Vec2D.RotatePointAroundPivotPoint(this.runtimMotor.Body.Center, this.runtimMotor.Body.Center + new Vec2D(20 - 10, 0 - 10), 320);

            panel.DrawLine(pen, p.ToGrx(), dir1.ToGrx());
            panel.DrawLine(pen, p.ToGrx(), dir2.ToGrx());
        }

        public bool IsPointInside(Vector2D point, Matrix4x4 screenToLocal)
        {
            screenToLocal *= Matrix4x4.Translate(sceneBoundingBox.X, sceneBoundingBox.Y, 0);
            point = Matrix4x4.MultPosition(screenToLocal, new Vector3D(point.X, point.Y, 0)).XY;

            return (this.runtimMotor.Body.Center.ToGrx() - point).Length() < 20;
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
