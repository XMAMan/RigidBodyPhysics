using GraphicMinimal;
using GraphicPanels;
using Part4.Model.Editor.EditorJoint;
using Part4.Model.Editor.EditorShape;
using PhysicEngine.MathHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Part4.Model.Editor.Function
{
    internal class AddDistanceJointFunction : IFunction
    {
        private enum Mode { SelectBody1, SelectBody2, PlaceAnchor1, PlaceAnchor2 }
        private Mode mode = Mode.SelectBody1;

        private List<IEditorShape> shapes = null;
        private List<IEditorJoint> joints = null;
        private Vector2D currentMousePosition = new Vector2D(0, 0);
        private IEditorShape selectedShape = null;

        private IEditorShape shape1 = null;
        private IEditorShape shape2 = null;
        private Vector2D anchor1 = null;
        private Vector2D anchor2 = null;

        public IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            this.shapes = shapes;
            this.joints = joints;
            return this;
        }

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
        }

        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
        }

        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                HandleLeftClick();

            if (e.Button == MouseButtons.Right)
                HandleRightClick();
        }

        private void HandleLeftClick()
        {
            switch (this.mode)
            {
                case Mode.SelectBody1:
                    {
                        if (this.selectedShape != null)
                        {
                            this.shape1 = this.selectedShape;
                            this.mode = Mode.SelectBody2;
                            this.shape1.Backcolor = Color.AliceBlue;
                            this.selectedShape = null;
                        }
                    }
                    break;

                case Mode.SelectBody2:
                    {
                        if (this.selectedShape != null)
                        {
                            this.shape2 = this.selectedShape;
                            this.mode = Mode.PlaceAnchor1;
                            this.shape2.Backcolor = Color.AliceBlue;
                        }
                    }
                    break;

                case Mode.PlaceAnchor1:
                    {
                        if (this.shape1.IsPointInside(this.currentMousePosition))
                        {
                            this.anchor1 = new Vector2D(this.currentMousePosition);
                            this.mode = Mode.PlaceAnchor2;
                        }                        
                    }
                    break;

                case Mode.PlaceAnchor2:
                    {
                        if (this.shape2.IsPointInside(this.currentMousePosition))
                        {
                            this.anchor2 = new Vector2D(this.currentMousePosition);
                            this.mode = Mode.SelectBody1;

                            Vector2D r1 = EditorShapeHelper.GetLocalBodyDirection(this.shape1, this.anchor1);
                            Vector2D r2 = EditorShapeHelper.GetLocalBodyDirection(this.shape2, this.anchor2);
                            this.joints.Add(new EditorDistanceJoint(this.shape1, this.shape2, r1, r2));

                            this.shape1 = null;
                            this.shape2 = null;
                            this.anchor1 = null;
                            this.anchor2 = null;
                        }                        
                    }
                    break;
            }
        }

        private void HandleRightClick()
        {
            switch (this.mode)
            {
                case Mode.SelectBody1:
                    {
                        //Do nothing
                    }
                    break;

                case Mode.SelectBody2:
                    {
                        if (this.shape1 != null)
                        {
                            this.shape1.Backcolor = Color.Transparent;
                            this.shape1 = null;
                            this.mode = Mode.SelectBody1;                            
                        }
                    }
                    break;

                case Mode.PlaceAnchor1:
                    {
                        if (this.shape2 != null)
                        {
                            this.shape2.Backcolor = Color.Transparent;
                            this.shape2 = null;
                            this.mode = Mode.SelectBody2;
                        }
                    }
                    break;

                case Mode.PlaceAnchor2:
                    {
                        this.anchor1 = null;
                        this.mode = Mode.PlaceAnchor1;
                    }
                    break;
            }
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            this.currentMousePosition = new Vector2D(e.X, e.Y);

            this.selectedShape = null;

            if (this.mode == Mode.SelectBody1 || this.mode == Mode.SelectBody2)
            {
                Vector2D mousePosition = new Vector2D(e.X, e.Y);
                foreach (var shape in this.shapes)
                {
                    if (shape != this.shape1 && shape != this.shape2)
                    {
                        if (shape.IsPointInside(mousePosition))
                        {
                            this.selectedShape = shape;
                            shape.Backcolor = Color.Red;
                        }
                        else
                        {
                            shape.Backcolor = Color.Transparent;
                        }
                    }
                }
            }

            if (this.mode == Mode.PlaceAnchor1)
            {

            }
            
        }
        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
        }

        public void Draw(GraphicPanel2D panel)
        {
            if (this.anchor1 != null)
            {
                panel.DrawFillCircle(Color.DarkGreen, this.anchor1, 3);
            }

            if (this.mode == Mode.PlaceAnchor1)
            {
                bool isInside = this.shape1.IsPointInside(this.currentMousePosition);
                panel.DrawFillCircle(isInside ? Color.DarkGreen : Color.Red, this.currentMousePosition, 3);
            }

            if (this.mode == Mode.PlaceAnchor2)
            {
                bool isInside = this.shape2.IsPointInside(this.currentMousePosition);
                panel.DrawFillCircle(isInside ? Color.DarkGreen : Color.Red, this.currentMousePosition, 3);
            }
        }

        public FunctionHelpText GetHelpText()
        {
            switch (this.mode)
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
                        Headline = "Position anchor point 1",
                        Values = new[]
                        {
                            "CLICK: place anchor point 1",
                            "RIGHT CLICK: unselect second shape",
                            "ESC: exit"
                        }
                    };

                case Mode.PlaceAnchor2:
                    return new FunctionHelpText()
                    {
                        Headline = "Position anchor point 2",
                        Values = new[]
                        {
                            "CLICK: place anchor point 2",
                            "RIGHT CLICK: unselect anchor point 1",
                            "ESC: exit"
                        }
                    };
            }

            throw new Exception("Unknown enum value:" + this.mode);
        }

        public void Dispose()
        {
            foreach (var shape in this.shapes)
                shape.Backcolor = Color.Transparent;
        }
    }
}
