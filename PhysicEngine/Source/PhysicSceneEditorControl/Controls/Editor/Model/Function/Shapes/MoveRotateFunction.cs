namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Shapes
{
    internal class MoveRotateFunction : MoveRotateResize, IFunction
    {
        public override IFunction Init(FunctionData functionData)
        {
            this.shapes = functionData.Shapes;
            this.joints = functionData.Joints;
            this.thrusters = functionData.Thrusters;
            this.axialFrictions = functionData.AxialFrictions;
            this.mouseGrid = functionData.MouseGrid;
            return this;
        }
        public override void HandleMouseWheel(MouseEventArgs e)
        {
            if (selectedShape != null)
            {
                if (shiftIsPressed)
                    selectedShape.Rotate(e.Delta / 10);
                else
                    selectedShape.Rotate(e.Delta / 150f);

                UpdateJointsAndThrusters(this.selectedShape);
            }
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Move and Rotate",
                Values = new[]
                    {
                        "CLICK AND DRAG: move shape or joint",
                        "MOUSE WHEEL: rotate shape",
                        "HOLD SHIFT: increase rotation speed",
                        "ESC: exit"
                    }
            };
        }
    }
}
