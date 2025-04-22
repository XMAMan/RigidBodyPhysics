using GraphicMinimal;
using GraphicPanels;
using Part4.Model.Editor.EditorJoint;
using Part4.Model.Editor.EditorShape;
using Part4.ViewModel.Editor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Part4.Model.Editor.Function
{
    class EditPropertiesFunction : IFunction
    {
        private List<IEditorShape> shapes = null;
        private List<IEditorJoint> joints = null;
        private IEditorShape shapeWhereMouseIsOver = null;
        private IEditorShape selectedShape = null;
        private IEditorJoint jointWhereMouseIsOver = null;
        private IEditorJoint selectedJoint = null;
        private Action<IEditorShape> shapeSelectedHandler;
        private Action<IEditorJoint> jointSelectedHandler;

        public IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            this.shapes = shapes;
            this.joints = joints;
            return this;
        }

        //Später soll hier noch ein jointSelectedHandler dazu kommen
        public IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints, Action<IEditorShape> shapeSelectedHandler, Action<IEditorJoint> jointSelectedHandler)
        {
            this.shapes = shapes;
            this.joints = joints;
            this.shapeSelectedHandler = shapeSelectedHandler;
            this.jointSelectedHandler = jointSelectedHandler;
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
            if (this.shapeWhereMouseIsOver != null)
            {
                if (this.selectedShape != null)
                    this.selectedShape.BorderPen = Pens.Black; //Deselect old Shape

                if (this.selectedJoint != null)
                    this.selectedJoint.BorderPen = Pens.Blue;  //Deselect old Joint

                this.selectedShape = this.shapeWhereMouseIsOver;
                this.shapeSelectedHandler(this.shapeWhereMouseIsOver);
                this.selectedShape.BorderPen = new Pen(Color.Blue, 3);
            }

            if (this.jointWhereMouseIsOver != null)
            {
                if (this.selectedShape != null)
                    this.selectedShape.BorderPen = Pens.Black; //Deselect old Shape

                if (this.selectedJoint != null)
                    this.selectedJoint.BorderPen = Pens.Blue;  //Deselect old Joint

                this.selectedJoint = this.jointWhereMouseIsOver;
                this.jointSelectedHandler(this.jointWhereMouseIsOver);
                this.selectedJoint.BorderPen = new Pen(Color.DarkGreen, 3);
            }
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            Vector2D mousePosition = new Vector2D(e.X, e.Y);

            this.shapeWhereMouseIsOver = null;            
            foreach (var shape in this.shapes)
            {
                if (shape.IsPointInside(mousePosition))
                {
                    this.shapeWhereMouseIsOver = shape;
                    shape.Backcolor = Color.Red;                    
                }
                else
                {
                    shape.Backcolor = Color.Transparent;
                }
            }

            this.jointWhereMouseIsOver = null;
            foreach (var joint in this.joints)
            {
                if (joint.IsPointInside(mousePosition))
                {
                    this.jointWhereMouseIsOver = joint;
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
                Headline = "Edit Properties",
                Values = new[]
                    {
                        "CLICK: edit shape or joint",
                        "ESC: exit"
                    }
            };
        }

        public void Dispose()
        {
            foreach (var shape in this.shapes)
            {
                shape.Backcolor = Color.Transparent;
                shape.BorderPen = Pens.Black;
            }
            foreach (var joint in this.joints)
            {
                joint.Backcolor = Color.Transparent;
                joint.BorderPen = Pens.Blue;
            }

            this.shapeSelectedHandler(null);
        }
    }
}
