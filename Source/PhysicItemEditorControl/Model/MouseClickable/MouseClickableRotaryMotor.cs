using LevelEditorExports.Simulator;
using LevelEditorGlobal;
using PhysicGlobal;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using System.Drawing;

namespace PhysicItemEditorControl.Model.MouseClickable
{
    internal class MouseClickableRotaryMotor : IMouseclickableWithTagData
    {
        private IPublicRotaryMotor runtimMotor; //Läßt sich leichter zeichnen
        private PhysicGlobal.BoundingBox sceneBoundingBox;

        public MouseClickableRotaryMotor(IPublicRotaryMotor runtimMotor, PhysicGlobal.BoundingBox sceneBoundingBox, int bodyIndex)
        {
            this.sceneBoundingBox = sceneBoundingBox;
            this.runtimMotor = runtimMotor;
            Id = bodyIndex;
        }

        public int Id { get; } //ITagable
        public TagType TypeName { get => TagType.Motor; } //ITagable

        public void Draw(IDrawingPanel panel)
        {
            DrawBorder(panel, Pens.Blue);
        }
        public void DrawBorder(IDrawingPanel panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-sceneBoundingBox.X, -sceneBoundingBox.Y, 0));
            DrawCircle(borderPen, panel);

            panel.PopMatrix();
        }

        private void DrawCircle(Pen pen, IDrawingPanel panel)
        {
            panel.DrawCircleArc(pen, this.runtimMotor.Body.Center, 20, 30, 320, false);
            var p = Vec2D.RotatePointAroundPivotPoint(this.runtimMotor.Body.Center, this.runtimMotor.Body.Center + new Vec2D(20, 0), 320);
            var dir1 = Vec2D.RotatePointAroundPivotPoint(this.runtimMotor.Body.Center, this.runtimMotor.Body.Center + new Vec2D(20 + 10, 0 - 10), 320);
            var dir2 = Vec2D.RotatePointAroundPivotPoint(this.runtimMotor.Body.Center, this.runtimMotor.Body.Center + new Vec2D(20 - 10, 0 - 10), 320);

            panel.DrawLine(pen, p, dir1);
            panel.DrawLine(pen, p, dir2);
        }

        public bool IsPointInside(Vec2D point, Matrix4x4 screenToLocal)
        {
            screenToLocal *= Matrix4x4.Translate(sceneBoundingBox.X, sceneBoundingBox.Y, 0);
            point = Matrix4x4.MultPosition(screenToLocal, point);

            return (this.runtimMotor.Body.Center - point).Length() < 20;
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
