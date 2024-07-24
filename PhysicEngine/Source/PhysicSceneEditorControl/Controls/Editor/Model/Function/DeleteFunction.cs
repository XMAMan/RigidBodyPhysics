using PhysicSceneEditorControl.Controls.Editor.Model.EditorAxialFriction;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorJoint;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorRotaryMotor;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorThruster;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function
{
    internal class DeleteFunction : DummyFunction, IFunction
    {
        private List<IEditorShape> shapes = null;
        private List<IEditorJoint> joints = null;
        private List<IEditorThruster> thrusters = null;
        private List<IEditorRotaryMotor> motors = null;
        private List<IEditorAxialFriction> axialFrictions = null;

        private IEditorShape selectedShape = null;
        private IEditorJoint selectedJoint = null;
        private IEditorThruster selectedThruster = null;
        private IEditorRotaryMotor selectedMotor = null;
        private IEditorAxialFriction selectedAxialFriction = null;
        private bool shiftIsPressed = false;

        public override IFunction Init(FunctionData functionData)
        {
            this.shapes = functionData.Shapes;
            this.joints = functionData.Joints;
            this.thrusters = functionData.Thrusters;
            this.motors = functionData.RotaryMotors;
            this.axialFrictions = functionData.AxialFrictions;
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

        public override void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.selectedShape != null)
            {
                this.shapes.Remove(this.selectedShape);

                if (this.joints.Any(x => x.Body1 == this.selectedShape || x.Body2 == this.selectedShape))
                {
                    var jointsToDelete = this.joints.Where(x => x.Body1 == this.selectedShape || x.Body2 == this.selectedShape).ToList();
                    foreach (var joint in jointsToDelete)
                    {
                        this.joints.Remove(joint);
                    }
                }

                if (this.thrusters.Any(x => x.Body == this.selectedShape))
                {
                    var thrustersToDelete = this.thrusters.Where(x => x.Body == this.selectedShape).ToList();
                    foreach (var thruster in thrustersToDelete)
                    {
                        this.thrusters.Remove(thruster);
                    }
                }

                if (this.motors.Any(x => x.Body == this.selectedShape))
                {
                    var motorsToDelete = this.motors.Where(x => x.Body == this.selectedShape).ToList();
                    foreach (var motor in motorsToDelete)
                    {
                        this.motors.Remove(motor);
                    }
                }

                if (this.axialFrictions.Any(x => x.Body == this.selectedShape))
                {
                    var axialFrictionsToDelete = this.axialFrictions.Where(x => x.Body == this.selectedShape).ToList();
                    foreach (var axialFriction in axialFrictionsToDelete)
                    {
                        this.axialFrictions.Remove(axialFriction);
                    }
                }
            }

            if (this.selectedJoint != null)
            {
                this.joints.Remove(this.selectedJoint);
            }

            if (this.selectedThruster != null)
            {
                this.thrusters.Remove(this.selectedThruster);
            }

            if (this.selectedMotor != null)
            {
                this.motors.Remove(this.selectedMotor);
            }

            if (this.selectedAxialFriction != null)
            {
                this.axialFrictions.Remove(this.selectedAxialFriction);
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            Vec2D mousePosition = new Vec2D(e.X, e.Y);

            this.selectedShape = null;
            if (this.shiftIsPressed == false)
            {
                this.selectedShape = ShapeHelper.GetShapeFromPoint(shapes, mousePosition);
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

            this.selectedThruster = null;
            foreach (var thruster in this.thrusters)
            {
                if (thruster.IsPointInside(mousePosition))
                {
                    this.selectedThruster = thruster;
                    thruster.Backcolor = Color.Red;
                }
                else
                {
                    thruster.Backcolor = Color.Transparent;
                }
            }

            this.selectedMotor = null;
            foreach (var motor in this.motors)
            {
                if (motor.IsPointInside(mousePosition))
                {
                    this.selectedMotor = motor;
                    motor.Backcolor = Color.Red;
                }
                else
                {
                    motor.Backcolor = Color.Transparent;
                }
            }

            this.selectedAxialFriction = null;
            foreach (var axialFriction in this.axialFrictions)
            {
                if (axialFriction.IsPointInside(mousePosition))
                {
                    this.selectedAxialFriction = axialFriction;
                    axialFriction.Backcolor = Color.Red;
                }else
                {
                    axialFriction.Backcolor= Color.Transparent;
                }
            }
        }

        public override FunctionHelpText GetHelpText()
        {
            return new FunctionHelpText()
            {
                Headline = "Delete shapes and joints",
                Values = new[]
                    {
                        "CLICK: delete shape, joint, thruster, motor or axial friction",
                        "Hold Shift: select joint, thruster motor or axial friction only",
                        "ESC: exit"
                    }
            };
        }

        public override void Dispose()
        {
            foreach (var shape in this.shapes)
                shape.Backcolor = Color.Transparent;

            foreach (var joint in this.joints)
                joint.Backcolor = Color.Transparent;

            foreach (var thruster in this.thrusters)
                thruster.Backcolor = Color.Transparent;

            foreach (var motor in this.motors)
                motor.Backcolor = Color.Transparent;

            foreach (var axialFriction in this.axialFrictions)
                axialFriction.Backcolor = Color.Transparent;
        }
    }
}
