using GraphicPanels;
using LevelEditorExports.Editor.Helper;
using LevelEditorExports.Simulator;
using LevelEditorGlobal;
using PhysicGlobal;
using System.Drawing;
using WpfControls.Extensions;

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
            panel.MultTransformationMatrix(this.rotatedRectangle.GetLocalToScreenMatrix().To4x4Matrix());
            this.decoree.Draw(panel);
            panel.PopMatrix();
        }
        public void DrawBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(this.rotatedRectangle.GetLocalToScreenMatrix().To4x4Matrix());
            this.decoree.DrawBorder(panel, borderPen);
            panel.PopMatrix();
        }

        public float GetArea()
        {
            return this.decoree.GetArea();
        }

        //screenToLocal = ScreenToCamera-Space-Matrix
        public bool IsPointInside(Vec2D point, PhxMatrix screenToLocal)
        {
            screenToLocal *= PhxMatrix.Invert(this.rotatedRectangle.GetLocalToScreenMatrix());  //Camera to LevelItem
            return this.decoree.IsPointInside(point, screenToLocal);
        }

        public PhxMatrix GetScreenToLocalMatrix()
        {
            return PhxMatrix.Invert(this.rotatedRectangle.GetLocalToScreenMatrix()) * this.decoree.GetScreenToLocalMatrix();
        }
    }
}
