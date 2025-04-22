using GraphicPanels;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorAxialFriction;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function
{
    internal class AddAxialFrictionFunction : AddAnchorAndDirectionFunction, IFunction
    {
        private List<IEditorAxialFriction> axialFrictions = null;
        public override IFunction Init(FunctionData functionData)
        {
            base.Init(functionData);
            this.axialFrictions = functionData.AxialFrictions;
            return this;
        }

        protected override void CreateNewObject(IEditorShape shape, Vec2D r1, Vec2D forceDirection)
        {
            this.axialFrictions.Add(new EditorAxialFriction.EditorAxialFriction(shape, r1, forceDirection));
        }

        protected override void DrawObject(GraphicPanel2D panel, Vec2D position, Vec2D direction)
        {
            EditorAxialFriction.EditorAxialFriction.DrawStick(panel, position, direction, Pens.Blue);
        }
    }
}
