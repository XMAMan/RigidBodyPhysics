using PhysicSceneEditorControl.Controls.Editor.Model.EditorAxialFriction;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorJoint;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorRotaryMotor;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorThruster;
using WpfControls.Model;

namespace PhysicSceneEditorControl.Controls.Editor.Model.Function
{
    internal class FunctionData
    {
        private static int MatrixSize = 5;

        public List<IEditorShape> Shapes = new List<IEditorShape>();
        public List<IEditorJoint> Joints = new List<IEditorJoint>();
        public List<IEditorThruster> Thrusters = new List<IEditorThruster>();
        public List<IEditorRotaryMotor> RotaryMotors = new List<IEditorRotaryMotor>();
        public List<IEditorAxialFriction> AxialFrictions = new List<IEditorAxialFriction>();
        public bool[,] CollisionMatrix = new bool[MatrixSize, MatrixSize];
        public MouseGrid MouseGrid = new MouseGrid() { ShowGrid = false };

        public FunctionData()
        {
            this.CollisionMatrix[0, 0] = true; //Objekte mit Kategory 0 sollen Defaultmäßig miteinander kollidieren
        }
    }
}
