using LevelEditorExports.Simulator;
using PhysicGlobal;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    //Wenn ein Backgrounditem innerhalb des GroupedItemsLevelItem vorkommt, dann müssen noch die beiden Matrizen aus dem GroupedLevelItem und dem GroupedProtoItem angwendet werden
    //Diese Klasse dekoriert ein IBackgroundItem um ein IDrawingPanel.MultTransformationMatrix-Aufruf
    internal class BackgroundItemDecorator : IBackgroundItem
    {
        private IBackgroundItem decoree;
        private Matrix4x4 matrix;
        public BackgroundItemDecorator(IBackgroundItem decoree, Matrix4x4 matrix)
        {
            this.decoree = decoree;
            this.matrix = matrix;
        }
        public void Draw(IDrawingPanel panel)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(this.matrix);
            this.decoree.Draw(panel);
            panel.PopMatrix();
        }

        public BackgroundItemSimulatorExportData GetSimulatorExportData()
        {
            var data = this.decoree.GetSimulatorExportData();

            float angleInDegreeMatrix = Matrix4x4.GetAngleInDegreeFromMatrix(matrix);
            float sizeFactorMatrix = Matrix4x4.GetSizeFactorFromMatrix(matrix);

            data.Center = Matrix4x4.MultPosition(matrix, data.Center);
            data.AngleInDegree += angleInDegreeMatrix;
            data.Width *= sizeFactorMatrix;
            data.Height *= sizeFactorMatrix;

            return data;
        }
    }
}
