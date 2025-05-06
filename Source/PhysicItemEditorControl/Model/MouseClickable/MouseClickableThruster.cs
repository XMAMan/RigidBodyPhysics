using GraphicMinimal;
using GraphicPanels;
using LevelEditorGlobal;
using RigidBodyPhysics.RuntimeObjects.Thruster;
using System.Drawing;
using WpfControls.Extensions;
using PhysicGlobal;
using LevelEditorExports.Simulator;

namespace PhysicItemEditorControl.Model.MouseClickable
{
    internal class MouseClickableThruster : IMouseclickableWithTagData
    {
        private IPublicThruster runtimThruster; //Läßt sich leichter zeichnen
        private PhysicGlobal.BoundingBox sceneBoundingBox;

        public MouseClickableThruster(IPublicThruster runtimThruster, PhysicGlobal.BoundingBox sceneBoundingBox, int bodyIndex)
        {
            this.sceneBoundingBox = sceneBoundingBox;
            this.runtimThruster = runtimThruster;
            Id = bodyIndex;
        }

        public int Id { get; } //ITagable
        public TagType TypeName { get => TagType.Thruster; } //ITagable

        public void Draw(GraphicPanel2D panel)
        {
            DrawBorder(panel, Pens.Blue);
        }
        public void DrawBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-sceneBoundingBox.X, -sceneBoundingBox.Y, 0));
            DrawArrow(panel, borderPen);

            panel.PopMatrix();
        }

        private void DrawArrow(GraphicPanel2D panel, Pen pen)
        {
            var dir = runtimThruster.ForceDirection.ToGrx();
            var pos = runtimThruster.Anchor.ToGrx();
            float r = 50;
            var v1 = Vector2D.GetV2FromAngle360(dir, 45 + 90);
            var v2 = Vector2D.GetV2FromAngle360(dir, -45 - 90);

            panel.DrawLine(pen, (pos - dir * r), pos);
            panel.DrawLine(pen, pos, pos + v1 * (r / 3));
            panel.DrawLine(pen, pos, pos + v2 * (r / 3));
        }

        public bool IsPointInside(Vec2D point, PhxMatrix screenToLocal)
        {
            screenToLocal *= PhxMatrix.Translate(sceneBoundingBox.X, sceneBoundingBox.Y, 0);
            point = PhxMatrix.MultPosition(screenToLocal, point);

            var dir = runtimThruster.ForceDirection;
            var pos = runtimThruster.Anchor;
            float r = 50;
            var v1 = Vec2D.GetV2FromAngle360(dir, 45 + 90);
            var v2 = Vec2D.GetV2FromAngle360(dir, -45 - 90);

            if (MathHelper.IsPointAboveLine(pos - dir * r, pos, point)) return true;
            if (MathHelper.IsPointAboveLine(pos, pos + v1 * (r / 3), point)) return true;
            if (MathHelper.IsPointAboveLine(pos, pos + v2 * (r / 3), point)) return true;
            return false;
        }

        public PhxMatrix GetScreenToLocalMatrix()
        {
            return PhxMatrix.Ident();
        }

        public float GetArea()
        {
            return 1;
        }
    }
}
