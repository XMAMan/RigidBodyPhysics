using GraphicPanels;
using LevelEditorExports.Simulator;
using PhysicGlobal;
using WpfControls.Extensions;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    //Wenn ein Backgrounditem innerhalb des GroupedItemsLevelItem vorkommt, dann müssen noch die beiden Matrizen aus dem GroupedLevelItem und dem GroupedProtoItem angwendet werden
    //Diese Klasse dekoriert ein IBackgroundItem um ein GraphicPanel2D.MultTransformationMatrix-Aufruf
    internal class BackgroundItemDecorator : IBackgroundItem
    {
        private IBackgroundItem decoree;
        private PhxMatrix matrix;
        public BackgroundItemDecorator(IBackgroundItem decoree, PhxMatrix matrix)
        {
            this.decoree = decoree;
            this.matrix = matrix;
        }
        public void Draw(GraphicPanel2D panel)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(this.matrix.To4x4Matrix());
            this.decoree.Draw(panel);
            panel.PopMatrix();
        }

        public BackgroundItemSimulatorExportData GetSimulatorExportData()
        {
            var data = this.decoree.GetSimulatorExportData();

            float angleInDegreeMatrix = PhxMatrix.GetAngleInDegreeFromMatrix(matrix);
            float sizeFactorMatrix = PhxMatrix.GetSizeFactorFromMatrix(matrix);

            data.Center = PhxMatrix.MultPosition(matrix, data.Center);
            data.AngleInDegree += angleInDegreeMatrix;
            data.Width *= sizeFactorMatrix;
            data.Height *= sizeFactorMatrix;

            return data;
        }
    }
}
