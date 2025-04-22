using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using GraphicPanels;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.Function.Joints
{
    //Selektiert zwei Shapes wo jede Shape noch ein Ankerpunkt hat
    internal abstract class PlaceTwoAnchorPointsFunction : DummyFunction, IFunction
    {
        protected enum Mode { SelectBody1, SelectBody2, PlaceAnchor1, PlaceAnchor2 }
        protected Mode mode = Mode.SelectBody1;

        protected List<IEditorShape> shapes = null;
        protected List<IEditorJoint> joints = null;
        private Vec2D currentMousePosition = new Vec2D(0, 0);
        private IEditorShape selectedShape = null;

        protected IEditorShape shape1 = null;
        protected IEditorShape shape2 = null;
        protected Vec2D anchor1 = null;
        protected Vec2D anchor2 = null;

        protected bool shiftIsPressed = false;

        private bool doPointIsInsideCheck;
        private bool showAxisOnBody1 = false;

        protected void Init(List<IEditorShape> shapes, List<IEditorJoint> joints, bool doPointIsInsideCheck, bool showAxisOnBody1)
        {
            this.shapes = shapes;
            this.joints = joints;
            this.doPointIsInsideCheck = doPointIsInsideCheck;
            this.showAxisOnBody1 = showAxisOnBody1;
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e) 
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = true;
        }
        public override void HandleKeyUp(System.Windows.Input.KeyEventArgs e) 
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = false;
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
                            mode = Mode.PlaceAnchor1;
                            shape2.Backcolor = Color.AliceBlue;
                        }
                    }
                    break;

                case Mode.PlaceAnchor1:
                    {
                        if (InsideCheck(shape1, currentMousePosition))
                        {
                            anchor1 = new Vec2D(currentMousePosition);
                            mode = Mode.PlaceAnchor2;
                        }
                    }
                    break;

                case Mode.PlaceAnchor2:
                    {
                        if (InsideCheck(shape2, currentMousePosition))
                        {
                            anchor2 = new Vec2D(currentMousePosition);
                            mode = Mode.SelectBody1;

                            Vec2D r1 = EditorShapeHelper.GetLocalBodyDirection(shape1, anchor1);
                            Vec2D r2 = EditorShapeHelper.GetLocalBodyDirection(shape2, anchor2);

                            CreateJoint(shape1, shape2, r1, r2);

                            shape1 = null;
                            shape2 = null;
                            anchor1 = null;
                            anchor2 = null;
                        }
                    }
                    break;
            }
        }

        protected abstract void CreateJoint(IEditorShape shape1, IEditorShape shape2, Vec2D r1, Vec2D r2);

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

                case Mode.PlaceAnchor1:
                    {
                        if (shape2 != null)
                        {
                            shape2.Backcolor = Color.Transparent;
                            shape2 = null;
                            mode = Mode.SelectBody2;
                        }
                    }
                    break;

                case Mode.PlaceAnchor2:
                    {
                        anchor1 = null;
                        mode = Mode.PlaceAnchor1;
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

            if (mode == Mode.PlaceAnchor1 && shiftIsPressed)
            {
                currentMousePosition = shape1.Center;
            }

            if (mode == Mode.PlaceAnchor2 && shiftIsPressed)
            {
                currentMousePosition = shape2.Center;
            }
        }
        public override void Draw(GraphicPanel2D panel)
        {
            if (anchor1 != null)
            {
                panel.DrawFillCircle(Color.DarkGreen, anchor1.ToGrx(), 3);
            }

            if (mode == Mode.PlaceAnchor1)
            {
                bool isInside = InsideCheck(shape1, currentMousePosition);
                panel.DrawFillCircle(isInside ? Color.DarkGreen : Color.Red, currentMousePosition.ToGrx(), 3);

                
                
            }

            if (mode == Mode.PlaceAnchor2)
            {
                bool isInside = InsideCheck(shape2, currentMousePosition);
                panel.DrawFillCircle(isInside ? Color.DarkGreen : Color.Red, currentMousePosition.ToGrx(), 3);
            }

            if (showAxisOnBody1 && (mode == Mode.PlaceAnchor1 || mode == Mode.PlaceAnchor2))
            {
                if (this.doPointIsInsideCheck && mode == Mode.PlaceAnchor1 && InsideCheck(shape1, currentMousePosition) == false)
                {
                    return; //Draw no Line if isInside is false durring Anchor1-Place
                }

                Vec2D p = mode == Mode.PlaceAnchor1 ? currentMousePosition : anchor1;
                Vec2D dir = (p - shape1.Center).Normalize();
                var box = shape1.GetBoundingBox();
                float r = box.Radius;
                panel.DrawLine(Pens.Blue, (shape1.Center - dir * r).ToGrx(), (shape1.Center + dir * r).ToGrx());
            }
        }

        private bool InsideCheck(IEditorShape shape, Vec2D position)
        {
            if (this.doPointIsInsideCheck == false) return true;
            return shape.IsPointInside(position);
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

                case Mode.PlaceAnchor1:
                    return new FunctionHelpText()
                    {
                        Headline = "Position anchor point 1" + (this.doPointIsInsideCheck ? " inside from Body1" : ""),
                        Values = new[]
                        {
                            "CLICK: place anchor point 1",
                            "RIGHT CLICK: unselect second shape",
                            "HOLD SHIFT: Place Anchorpoint1 in center",
                            "ESC: exit"
                        }
                    };

                case Mode.PlaceAnchor2:
                    return new FunctionHelpText()
                    {
                        Headline = "Position anchor point 2" + (this.doPointIsInsideCheck ? " inside from Body2" : ""),
                        Values = new[]
                        {
                            "CLICK: place anchor point 2",
                            "RIGHT CLICK: unselect anchor point 1",
                            "HOLD SHIFT: Place Anchorpoint2 in center",
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
