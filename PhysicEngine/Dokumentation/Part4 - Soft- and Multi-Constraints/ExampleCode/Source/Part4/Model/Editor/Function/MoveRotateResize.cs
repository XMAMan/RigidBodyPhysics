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
    abstract class MoveRotateResize : IFunction
    {
        protected List<IEditorShape> shapes = null;
        protected List<IEditorJoint> joints = null;
        protected bool shiftIsPressed = false;
        protected IEditorShape selectedShape = null;
        private Vector2D centerToMouseDown = null;

        public abstract IFunction Init(List<IEditorShape> shapes, List<IEditorJoint> joints);

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = true;
        }

        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = false;
        }
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public abstract void HandleMouseWheel(MouseEventArgs e);

        public void HandleMouseMove(MouseEventArgs e)
        {
            //Wenn die Taste nicht unten ist schaue ob die Mause über einen Objekt ist
            if (this.centerToMouseDown == null)
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
            }

            //Wenn die Taste unten ist und ein Objekt selektiert wurde dann verschiebe es
            if (this.selectedShape != null && this.centerToMouseDown != null)
            {
                this.selectedShape.MoveTo(new Vector2D(e.X, e.Y) - this.centerToMouseDown);
                var jointsToUpdate = this.joints.Where(x => x.Body1 == this.selectedShape || x.Body2 == this.selectedShape).ToList();
                jointsToUpdate.ForEach(x => x.UpdateAfterMovingBodys());
            }
        }

        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.selectedShape != null)
            {
                this.centerToMouseDown = new Vector2D(e.X, e.Y) - this.selectedShape.Center;
            }
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            this.selectedShape = null;
            this.centerToMouseDown = null;
        }

        public void Draw(GraphicPanel2D panel)
        {
        }

        public abstract FunctionHelpText GetHelpText();
        

        public void Dispose()
        {
            foreach (var shape in this.shapes)
                shape.Backcolor = Color.Transparent;
        }
    }
}
