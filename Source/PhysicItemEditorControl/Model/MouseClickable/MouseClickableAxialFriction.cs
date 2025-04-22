using GraphicMinimal;
using GraphicPanels;
using LevelEditorGlobal;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using System.Drawing;
using static LevelEditorGlobal.ITagable;

namespace PhysicItemEditorControl.Model.MouseClickable
{
    internal class MouseClickableAxialFriction : IMouseclickableWithTagData
    {
        private IPublicAxialFriction runtimAxialFriction; //Läßt sich leichter zeichnen
        private RectangleF sceneBoundingBox;

        public MouseClickableAxialFriction(IPublicAxialFriction runtimAxialFriction, RectangleF sceneBoundingBox, int bodyIndex)
        {
            this.sceneBoundingBox = sceneBoundingBox;
            this.runtimAxialFriction = runtimAxialFriction;
            Id = bodyIndex;
        }

        public int Id { get; } //ITagable
        public TagType TypeName { get => TagType.AxialFriction; } //ITagable

        public void Draw(GraphicPanel2D panel)
        {
            DrawBorder(panel, Pens.Blue);
        }
        public void DrawBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-sceneBoundingBox.X, -sceneBoundingBox.Y, 0));
            DrawStick(panel, this.runtimAxialFriction.Anchor, this.runtimAxialFriction.ForceDirection, borderPen);

            panel.PopMatrix();
        }

        private static void DrawStick(GraphicPanel2D panel, Vec2D position, Vec2D direction, Pen pen)
        {
            float r = 25;
            var p1 = position - direction * r;
            var p2 = position + direction * r;
            panel.DrawLine(pen, p1.ToGrx(), p2.ToGrx());

            int count = 5;
            float l = 10;
            Vec2D normal = direction.Spin90();
            for (int i = 0; i <= count; i++)
            {
                float f = (float)i / count;
                var p = (1 - f) * p1 + f * p2;
                panel.DrawLine(pen, (p - normal * l).ToGrx(), (p + normal * l).ToGrx());
            }
        }

        public bool IsPointInside(Vector2D point, Matrix4x4 screenToLocal)
        {
            screenToLocal *= Matrix4x4.Translate(sceneBoundingBox.X, sceneBoundingBox.Y, 0);
            point = Matrix4x4.MultPosition(screenToLocal, new Vector3D(point.X, point.Y, 0)).XY;

            float r = 25;
            var dir = runtimAxialFriction.ForceDirection.ToGrx();
            var pos = runtimAxialFriction.Anchor.ToGrx();
            var p1 = pos - dir * r;
            var p2 = pos + dir * r;
            return MathHelper.IsPointAboveLine(p1, p2, point);
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
