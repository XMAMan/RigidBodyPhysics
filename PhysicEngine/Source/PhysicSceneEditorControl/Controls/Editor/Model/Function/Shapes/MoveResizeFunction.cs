namespace PhysicSceneEditorControl.Controls.Editor.Model.Function.Shapes
{
    internal class MoveResizeFunction : MoveRotateResize, IFunction
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
                float size = Math.Min(1, Math.Max(-1, e.Delta / 150f)); //Clamp from -1 to +1

                if (shiftIsPressed)
                    selectedShape.Resize(1 + size / 2);
                else
                    selectedShape.Resize(1 + size / 20);

                UpdateJointsAndThrusters(this.selectedShape);
            }
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Move and Resize",
                Values = new[]
                    {
                        "CLICK AND DRAG: move shape or joint",
                        "MOUSE WHEEL: scale shape",
                        "HOLD SHIFT: increase scale speed",
                        "ESC: exit"
                    }
            };
        }
    }
}
