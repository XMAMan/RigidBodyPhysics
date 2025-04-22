using EditorControl.Model.EditorShape;
using EditorControl.Model.EditorThruster;
using GraphicPanels;
using PhysicEngine.MathHelper;

namespace EditorControl.Model.Function
{
    internal class AddThrusterFunction : DummyFunction, IFunction
    {
        private enum Mode { SelectBody, PlaceAnchor, PlaceDirection }
        private Mode mode = Mode.SelectBody;

        private List<IEditorShape> shapes = null;
        private List<IEditorThruster> thrusters = null;
        private Vec2D currentMousePosition = new Vec2D(0, 0);
        private IEditorShape selectedShape = null;
        private bool mouseIsPlacedOnSnapPoint = false;

        protected IEditorShape shape = null;
        protected Vec2D anchor = null;
        protected Vec2D direction = null;

        protected bool shiftIsPressed = false;

        public override IFunction Init(FunctionData functionData)
        {
            this.shapes = functionData.Shapes;
            this.thrusters = functionData.Thrusters;
            return this;
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
                case Mode.SelectBody:
                    {
                        if (selectedShape != null)
                        {
                            shape = selectedShape;
                            mode = Mode.PlaceAnchor;
                            shape.Backcolor = Color.AliceBlue;
                            selectedShape = null;
                        }
                    }
                    break;

                case Mode.PlaceAnchor:
                    {
                        if (InsideCheck(shape, currentMousePosition))
                        {
                            anchor = new Vec2D(currentMousePosition);
                            mode = Mode.PlaceDirection;
                        }
                    }
                    break;

                case Mode.PlaceDirection:
                    {
                        mode = Mode.SelectBody;
                        

                        Vec2D r1 = EditorShapeHelper.GetLocalBodyDirectionFromPosition(shape, anchor);
                        Vec2D forceDirection = EditorShapeHelper.GetLocalBodyDirectionFromWorldDirection(shape, this.direction);

                        CreateThruster(shape, r1, forceDirection);

                        shape = null;
                        anchor = null;
                        direction = null;
                    }
                    break;
            }
        }

        private void CreateThruster(IEditorShape shape, Vec2D r1, Vec2D forceDirection)
        {
            this.thrusters.Add(new EditorThruster.EditorThruster(shape, r1, forceDirection));
        }

        private void HandleRightClick()
        {
            switch (mode)
            {
                case Mode.SelectBody:
                    {
                        //Do nothing
                    }
                    break;

                case Mode.PlaceAnchor:
                    {
                        if (shape != null)
                        {
                            shape.Backcolor = Color.Transparent;
                            shape = null;
                            mode = Mode.SelectBody;
                        }
                    }
                    break;

                case Mode.PlaceDirection:
                    {
                        anchor = null;
                        mode = Mode.PlaceAnchor;
                    }
                    break;
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            currentMousePosition = new Vec2D(e.X, e.Y);

            selectedShape = null;

            if (mode == Mode.SelectBody)
            {
                Vec2D mousePosition = new Vec2D(e.X, e.Y);
                selectedShape = ShapeHelper.GetShapeFromPoint(shapes, mousePosition);
            }

            //Snap-Anchor-Points
            this.mouseIsPlacedOnSnapPoint = false;
            if (shiftIsPressed && mode == Mode.PlaceAnchor)
            {
                var points = this.shape.GetAnchorPoints();
                foreach (var point in points)
                {
                    bool mouseIsOverSnapPoint = (point - currentMousePosition).Length() < 5;
                    if (mouseIsOverSnapPoint)
                    {
                        currentMousePosition = point;
                        mouseIsPlacedOnSnapPoint = true;
                    }
                }
            }

            if (mode == Mode.PlaceDirection)
            {
                //Verschiebe nur Horizontal/Vertikal
                if (shiftIsPressed)
                {                    
                    Vec2D p1 = this.anchor;
                    Vec2D p2 = new Vec2D(e.X, e.Y);

                    Vec2D l = p2 - p1;

                    Vec2D p3 = null; //Korrigierte Mausposition

                    if (Math.Abs(l.X) > Math.Abs(l.Y))
                    {
                        //Verschiebe horizontal
                        p3 = new Vec2D(e.X, p1.Y);
                    }
                    else
                    {
                        //Verschiebe vertikal
                        p3 = new Vec2D(p1.X, e.Y);
                    }

                    this.currentMousePosition = p3;
                }
                
                this.direction = (this.anchor - currentMousePosition).Normalize();
            }
        }

        public override void Draw(GraphicPanel2D panel)
        {
            if (anchor != null)
            {
                panel.DrawFillCircle(Color.DarkGreen, anchor.ToGrx(), 3);
            }

            //Snap-Anchor-Points
            if (shiftIsPressed && mode == Mode.PlaceAnchor)
            {
                var points = this.shape.GetAnchorPoints();
                foreach (var point in points)
                {
                    bool mouseIsOverSnapPoint = (point - currentMousePosition).Length() < 5;
                    if (mouseIsOverSnapPoint)
                        panel.DrawFillCircle(Color.Yellow, point.ToGrx(), 5);

                    panel.DrawFillCircle(Color.Blue, point.ToGrx(), 3);
                }
            }

            if (mode == Mode.PlaceAnchor)
            {
                bool isInside = InsideCheck(shape, currentMousePosition);
                panel.DrawFillCircle(isInside ? Color.DarkGreen : Color.Red, currentMousePosition.ToGrx(), 3);
            }


            //Linie vom Anchor-Punkt zu MousePosition
            if (mode == Mode.PlaceDirection)
            {
                var dir = (this.anchor - currentMousePosition).Normalize();

                float r = 50;
                var v1 = GraphicMinimal.Vector2D.GetV2FromAngle360(dir.ToGrx(), 45 + 90);
                var v2 = GraphicMinimal.Vector2D.GetV2FromAngle360(dir.ToGrx(), -45 - 90);

                panel.DrawLine(Pens.Blue, (this.anchor - dir * r).ToGrx(), this.anchor.ToGrx());                
                panel.DrawLine(Pens.Blue, this.anchor.ToGrx(), this.anchor.ToGrx() + v1 * (r / 3));
                panel.DrawLine(Pens.Blue, this.anchor.ToGrx(), this.anchor.ToGrx() + v2 * (r / 3));
            }


        }

        private bool InsideCheck(IEditorShape shape, Vec2D position)
        {
            if (this.mouseIsPlacedOnSnapPoint) return true;

            return shape.IsPointInside(position);
        }

        public override FunctionHelpText GetHelpText()
        {
            switch (mode)
            {
                case Mode.SelectBody:
                    return new FunctionHelpText()
                    {
                        Headline = "Select Body",
                        Values = new[]
                        {
                            "CLICK: select shape",
                            "ESC: exit"
                        }
                    };


                case Mode.PlaceAnchor:
                    return new FunctionHelpText()
                    {
                        Headline = "Position anchor point inside from Body",
                        Values = new[]
                        {
                            "CLICK: place anchor point",
                            "RIGHT CLICK: unselect shape",
                            "HOLD SHIFT: Place Anchorpoint on Snappoint",
                            "ESC: exit"
                        }
                    };

                case Mode.PlaceDirection:
                    return new FunctionHelpText()
                    {
                        Headline = "Place Direction",
                        Values = new[]
                        {
                            "CLICK: place direction point",
                            "RIGHT CLICK: unselect anchor point",
                            "HOLD SHIFT: Place Direction Horizontal/Vertical",
                            "ESC: exit"
                        }
                    };
            }

            throw new Exception("Unknown enum value:" + mode);
        }
    }
}
