using GraphicMinimal;
using GraphicPanels;
using LevelEditorGlobal;
using System.Drawing;
using static LevelEditorGlobal.ITagable;

namespace LevelEditorControl.LevelItems.PhysicItem
{
    //Bekommt vom Prototyp die RigidBodys/Joints/Thruster/RotaryMotor und verschiebt sie laut LevelItem-Position
    //Diese Klasse ermöglicht, dass man ein RigidBody/Joint/Thruster mit der Maus anklicken kann und TagDaten dafür definieren kann
    internal class MouseClickableDecorator : IMouseClickable, IMouseclickableWithTagData
    {
        private IMouseclickableWithTagData decoree;
        private RotatedRectangle rotatedRectangle;
        public MouseClickableDecorator(IMouseClickable decoree, RotatedRectangle rotatedRectangle)
        {
            this.decoree = (IMouseclickableWithTagData)decoree;
            this.rotatedRectangle = rotatedRectangle;
        }
        public int Id { get => this.decoree.Id; } //ITagable
        public TagType TypeName { get => this.decoree.TypeName; } //ITagable

        public void Draw(GraphicPanel2D panel)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(this.rotatedRectangle.GetLocalToScreenMatrix());
            this.decoree.Draw(panel);
            panel.PopMatrix();
        }
        public void DrawBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(this.rotatedRectangle.GetLocalToScreenMatrix());
            this.decoree.DrawBorder(panel, borderPen);
            panel.PopMatrix();
        }

        public float GetArea()
        {
            return this.decoree.GetArea();
        }

        //screenToLocal = ScreenToCamera-Space-Matrix
        public bool IsPointInside(Vector2D point, Matrix4x4 screenToLocal)
        {
            screenToLocal *= Matrix4x4.Invert(this.rotatedRectangle.GetLocalToScreenMatrix());  //Camera to LevelItem
            return this.decoree.IsPointInside(point, screenToLocal);
        }

        public Matrix4x4 GetScreenToLocalMatrix()
        {
            return Matrix4x4.Invert(this.rotatedRectangle.GetLocalToScreenMatrix()) * this.decoree.GetScreenToLocalMatrix();
        }
    }
}
