using GraphicMinimal;
using GraphicPanels;
using Part4.Model.Editor.EditorJoint;
using Part4.Model.Editor.EditorShape;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Part4.Model.Editor.Function
{
    class DeleteFunction : IFunction
    {
        private List<IEditorShape> shapes = null;
        private List<IEditorJoint> joints = null;
        private IEditorShape selectedShape = null;
        private IEditorJoint selectedJoint = null;

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
            if (this.selectedShape != null)
            {
                this.shapes.Remove(this.selectedShape);

                if (this.joints.Any(x => x.Body1 == this.selectedShape ||  x.Body2 == this.selectedShape))
                {
                    var jointsToDelete = this.joints.Where(x => x.Body1 == this.selectedShape || x.Body2 == this.selectedShape).ToList();
                    foreach (var joint in jointsToDelete)
                    {
                        this.joints.Remove(joint);
                    }
                }
            }

            if (this.selectedJoint != null)
            {
                this.joints.Remove(this.selectedJoint);
            }
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            Vector2D mousePosition = new Vector2D(e.X, e.Y);

            this.selectedShape = null;
            foreach (var shape in this.shapes)
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

            this.selectedJoint = null;
            foreach (var joint in this.joints)
            {
                if (joint.IsPointInside(mousePosition))
                {
                    this.selectedJoint = joint;
                    joint.Backcolor = Color.Red;
                }
                else
                {
                    joint.Backcolor = Color.Transparent;
                }
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
        }

        public FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Delete shapes and joints",
                Values = new[]
                    {
                        "CLICK: delete shape or joint",
                        "ESC: exit"
                    }
            };
        }

        public void Dispose()
        {
            foreach (var shape in this.shapes)
                shape.Backcolor = Color.Transparent;

            foreach (var joint in this.joints)
                joint.Backcolor = Color.Transparent;
        }
    }
}
