using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorRotaryMotor;
using EditorControl.Model.EditorShape;
using EditorControl.Model.EditorThruster;
using WpfControls.Model;

namespace EditorControl.Model.Function
{
    internal class FunctionData
    {
        private static int MatrixSize = 5;

        public List<IEditorShape> Shapes = new List<IEditorShape>();
        public List<IEditorJoint> Joints = new List<IEditorJoint>();
        public List<IEditorThruster> Thrusters = new List<IEditorThruster>();
        public List<IEditorRotaryMotor> RotaryMotors = new List<IEditorRotaryMotor>();
        public bool[,] CollisionMatrix = new bool[MatrixSize, MatrixSize];
        public MouseGrid MouseGrid = new MouseGrid() { ShowGrid = false };

        public FunctionData()
        {
            this.CollisionMatrix[0, 0] = true; //Objekte mit Kategory 0 sollen Defaultmäßig miteinander kollidieren
        }
    }
}
