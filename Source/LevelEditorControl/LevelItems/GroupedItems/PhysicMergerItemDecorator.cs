using LevelEditorExports.Editor.Prototyps;
using LevelToSimulatorConverter._2_MergeToSingleScene;
using PhysicGlobal;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    internal class PhysicMergerItemDecorator : IMergeablePhysicScene
    {
        private IMergeablePhysicScene decoree;
        private PhxMatrix matrix;
        public PhysicMergerItemDecorator(IMergeablePhysicScene decoree, PhxMatrix matrix)
        {
            this.decoree = decoree;
            this.matrix = matrix;
        }

        public int LevelItemId => this.decoree.LevelItemId;

        public PhysicItemExportData PhysicData => this.decoree.PhysicData;
        public PhxMatrix GetTranslationMatrix()
        {
            return this.decoree.GetTranslationMatrix() * this.matrix;
        }
    }
}
