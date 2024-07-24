using GraphicMinimal;
using LevelToSimulatorConverter;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    internal class PhysicMergerItemDecorator : IMergeablePhysicScene
    {
        private IMergeablePhysicScene decoree;
        private Matrix4x4 matrix;
        public PhysicMergerItemDecorator(IMergeablePhysicScene decoree, Matrix4x4 matrix)
        {
            this.decoree = decoree;
            this.matrix = matrix;
        }

        public int LevelItemId => this.decoree.LevelItemId;

        public object PhysicData => this.decoree.PhysicData;
        public Matrix4x4 GetTranslationMatrix()
        {
            return this.decoree.GetTranslationMatrix() * this.matrix;
        }
    }
}
