using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using GraphicPanels;
using PhysicEngine.MathHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorControl.Model.Function.Joints
{
    internal abstract class PlaceOneAnchorPointFunction : DummyFunction, IFunction
    {
        protected enum Mode { SelectBody1, SelectBody2, PlaceAnchor }
        protected Mode mode = Mode.SelectBody1;

        protected List<IEditorShape> shapes = null;
        protected List<IEditorJoint> joints = null;
        protected Vec2D currentMousePosition = new Vec2D(0, 0);
        protected IEditorShape selectedShape = null;

        protected IEditorShape shape1 = null;
        protected IEditorShape shape2 = null;
        protected Vec2D anchor = null;

        protected void Init1(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            this.shapes = shapes;
            this.joints = joints;
        }

        public override void HandleMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                HandleLeftClick();

            if (e.Button == MouseButtons.Right)
                HandleRightClick();
        }

        private void HandleLeftClick()
        {
            switch (mode)
            {
                case Mode.SelectBody1:
                    {
                        if (selectedShape != null)
                        {
                            shape1 = selectedShape;
                            mode = Mode.SelectBody2;
                            shape1.Backcolor = Color.AliceBlue;
                            selectedShape = null;
                        }
                    }
                    break;

                case Mode.SelectBody2:
                    {
                        if (selectedShape != null)
                        {
                            shape2 = selectedShape;
                            mode = Mode.PlaceAnchor;
                            shape2.Backcolor = Color.AliceBlue;
                        }
                    }
                    break;

                case Mode.PlaceAnchor:
                    {
                        anchor = new Vec2D(currentMousePosition);
                        mode = Mode.SelectBody1;

                        CreateJoint(shape1, shape2, anchor);
                        

                        shape1 = null;
                        shape2 = null;
                        anchor = null;
                    }
                    break;
            }
        }

        protected abstract void CreateJoint(IEditorShape shape1, IEditorShape shape2, Vec2D anchorWorldPosition);

        private void HandleRightClick()
        {
            switch (mode)
            {
                case Mode.SelectBody1:
                    {
                        //Do nothing
                    }
                    break;

                case Mode.SelectBody2:
                    {
                        if (shape1 != null)
                        {
                            shape1.Backcolor = Color.Transparent;
                            shape1 = null;
                            mode = Mode.SelectBody1;
                        }
                    }
                    break;

                case Mode.PlaceAnchor:
                    {
                        if (shape2 != null)
                        {
                            shape2.Backcolor = Color.Transparent;
                            shape2 = null;
                            mode = Mode.SelectBody2;
                        }
                    }
                    break;
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            currentMousePosition = new Vec2D(e.X, e.Y);

            selectedShape = null;

            if (mode == Mode.SelectBody1 || mode == Mode.SelectBody2)
            {
                Vec2D mousePosition = new Vec2D(e.X, e.Y);
                foreach (var shape in shapes)
                {
                    if (shape != shape1 && shape != shape2)
                    {
                        if (shape.IsPointInside(mousePosition))
                        {
                            selectedShape = shape;
                            shape.Backcolor = Color.Red;
                        }
                        else
                        {
                            shape.Backcolor = Color.Transparent;
                        }
                    }
                }
            }

        }
        public override void Draw(GraphicPanel2D panel)
        {
            if (anchor != null)
            {
                panel.DrawFillCircle(Color.DarkGreen, anchor.ToGrx(), 3);
            }

            if (mode == Mode.PlaceAnchor)
            {
                panel.DrawFillCircle(Color.DarkGreen, currentMousePosition.ToGrx(), 3);
            }
        }

        public override FunctionHelpText GetHelpText()
        {
            switch (mode)
            {
                case Mode.SelectBody1:
                    return new FunctionHelpText()
                    {
                        Headline = "Select Body1",
                        Values = new[]
                        {
                            "CLICK: select shape",
                            "ESC: exit"
                        }
                    };

                case Mode.SelectBody2:
                    return new FunctionHelpText()
                    {
                        Headline = "Select Body2",
                        Values = new[]
                        {
                            "CLICK: select shape",
                            "RIGHT CLICK: unselect first shape",
                            "ESC: exit"
                        }
                    };

                case Mode.PlaceAnchor:
                    return new FunctionHelpText()
                    {
                        Headline = "Position anchor point",
                        Values = new[]
                        {
                            "CLICK: place anchor point",
                            "RIGHT CLICK: unselect second shape",
                            "ESC: exit"
                        }
                    };
            }

            throw new Exception("Unknown enum value:" + mode);
        }

        public override void Dispose()
        {
            foreach (var shape in shapes)
                shape.Backcolor = Color.Transparent;
        }
    }
}
