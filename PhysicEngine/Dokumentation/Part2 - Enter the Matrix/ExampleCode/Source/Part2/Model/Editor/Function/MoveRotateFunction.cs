﻿using GraphicMinimal;
using GraphicPanels;
using Part2.Model.Editor.EditorShape;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Part2.Model.Editor.Function
{
    class MoveRotateFunction : MoveRotateResize, IFunction
    {
        public override IFunction Init(List<IEditorShape> shapes)
        {
            this.shapes = shapes;
            return this;
        }
        public override void HandleMouseWheel(MouseEventArgs e)
        {
            if (this.selectedShape != null)
            {
                if (this.shiftIsPressed)
                    this.selectedShape.Rotate(e.Delta / 10);
                else
                    this.selectedShape.Rotate(e.Delta / 150f);
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