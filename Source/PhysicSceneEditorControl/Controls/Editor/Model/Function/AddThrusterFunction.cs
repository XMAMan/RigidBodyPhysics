using GraphicPanels;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorThruster;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function
{
    internal class AddThrusterFunction : AddAnchorAndDirectionFunction, IFunction
    {
        private List<IEditorThruster> thrusters = null;
        public override IFunction Init(FunctionData functionData)
        {
            base.Init(functionData);
            this.thrusters = functionData.Thrusters;
            return this;
        }

        protected override void CreateNewObject(IEditorShape shape, Vec2D r1, Vec2D forceDirection)
        {
            this.thrusters.Add(new EditorThruster.EditorThruster(shape, r1, forceDirection));
        }

        protected override void DrawObject(GraphicPanel2D panel, Vec2D position, Vec2D direction)
        {
            EditorThruster.EditorThruster.DrawArrow(panel, position, direction, Pens.Blue);
        }
    }
}
