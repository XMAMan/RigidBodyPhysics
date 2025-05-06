using LevelEditorExports.Editor.LevelItems;
using LevelEditorExports.Editor;
using PhysicGlobal;
using LevelToSimulatorConverter.Helper;
using LevelEditorExports.Editor.Helper;
using LevelEditorExports.Editor.Prototyps;
using LevelEditorExports.Simulator;
using System.Drawing;
using LevelToSimulatorConverter._2_MergeToSingleScene;

namespace LevelToSimulatorConverter._1_LokalToWorldTransform
{
    //Berechnet die Boundingbox von LevelExport-Items
    internal static class BoundingBoxHelper
    {
        public static BoundingBox GetBoundingBoxFromPhysicLevelItem(PhysicLevelItemExportData item, LevelEditorExportData levelExport)
        {
            var convertedItem = PhysicExportToMergerItemConverter.ConvertPhysicLevelItem(item, levelExport);
            return GetBoundingBoxFromLevelItem(convertedItem);
        }

        private static BoundingBox GetBoundingBoxFromLevelItem(IMergeablePhysicScene item)
        {
            var copy = PhysicSceneExportDataHelper.CreateCopyFromScene(item.PhysicData.PhysicSceneData);
            PhysicSceneExportDataHelper.TranslateScene(copy, item.GetTranslationMatrix());
            return PhysicSceneExportDataHelper.GetBoundingBoxFromScene(copy);
        }

        public static BoundingBox GetBoundingBoxFromBackgroundItem(BackgroundItemSimulatorExportData item)
        {
            var initialRecValues = new InitialRotatedRectangleValues()
            {
                SizeFactor = 1,
                AngleInDegree = item.AngleInDegree,
                LocalPivot = new Vec2D(0.5f, 0.5f)
            };
            var rotRec = new RotatedRectangle(item.Center, new SizeF(item.Width, item.Height), initialRecValues);

            var cornerPoints = rotRec.GetCornerPoints();

            return BoundingBox.GetBoxFromPoints(cornerPoints);
        }

        public static BoundingBox GetBoundingBoxFromGroupedItem(GroupedItemProtoExportData item, LevelEditorExportData levelExport)
        {
            return BoundingBox.GetBoxFromBoxes(BoundingBoxHelper.GetBoundingBoxesFromLevelItemList(item.LevelItemsExport, levelExport));
        }

        private static BoundingBox[] GetBoundingBoxesFromLevelItemList(ILevelItemExportData[] levelItems, LevelEditorExportData levelExport)
        {
            List<BoundingBox> returnList = new List<BoundingBox>();

            foreach (var item in levelItems)
            {
                if (item is PhysicLevelItemExportData)
                {
                    returnList.Add(BoundingBoxHelper.GetBoundingBoxFromPhysicLevelItem((PhysicLevelItemExportData)item, levelExport));
                }

                if (item is PolygonLevelItemExportData)
                {
                    returnList.Add(BoundingBox.GetBoxFromPoints(((PolygonLevelItemExportData)item).Points));
                }

                if (item is BackgroundLevelItemExportData)
                {
                    var backgroundItem = BackgroundExportToSimulatorConverter.ConvertBackgroundItem((BackgroundLevelItemExportData)item, levelExport);
                    returnList.Add(BoundingBoxHelper.GetBoundingBoxFromBackgroundItem(backgroundItem));
                }

                if (item is LawnEdgeExportData)
                {
                    var lawnEdgeItems = BackgroundExportToSimulatorConverter.ConvertLawnEdge((LawnEdgeExportData)item, levelExport);
                    returnList.AddRange(lawnEdgeItems.Select(x => BoundingBoxHelper.GetBoundingBoxFromBackgroundItem(x)));
                }

                if (item is GroupedItemLevelExportData)
                {
                    var groupedItem = (GroupedItemLevelExportData)item;
                    var protoData = (GroupedItemProtoExportData)levelExport.Prototyps.PrototypItems.First(x => x.Id == groupedItem.PrototypId);
                    returnList.AddRange(GetBoundingBoxesFromLevelItemList(protoData.LevelItemsExport, levelExport));
                }
            }

            return returnList.ToArray();
        }
    }
}
