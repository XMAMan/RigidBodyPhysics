using LevelEditorExports.Editor.LevelItems;
using LevelEditorExports.Editor;
using LevelEditorExports.Simulator;
using LevelEditorExports.Editor.Prototyps;
using PhysicGlobal;
using System.Drawing;
using LevelEditorGlobal.Helper;

namespace LevelToSimulatorConverter._1_LokalToWorldTransform
{
    internal static class BackgroundExportToSimulatorConverter
    {
        public static BackgroundItemSimulatorExportData[] GetBackgroundItemsFromExportScene(LevelEditorExportData levelExport)
        {
            return GetBackgroundItemsFromLevelItems(levelExport.LevelItems, levelExport);
        }

        private static BackgroundItemSimulatorExportData[] GetBackgroundItemsFromLevelItems(ILevelItemExportData[] levelItems, LevelEditorExportData levelExport)
        {
            List<BackgroundItemSimulatorExportData> returnList = new List<BackgroundItemSimulatorExportData>();

            foreach (var item in levelItems)
            {
                if (item is BackgroundLevelItemExportData)
                {
                    var backgroundItem = ConvertBackgroundItem((BackgroundLevelItemExportData)item, levelExport);
                    returnList.Add(backgroundItem);
                }

                if (item is LawnEdgeExportData)
                {
                    var lawnEdgeItem = ConvertLawnEdge((LawnEdgeExportData)item, levelExport);
                    returnList.AddRange(lawnEdgeItem);
                }

                if (item is GroupedItemLevelExportData)
                {
                    var groupedItems = ConvertGroupedItem((GroupedItemLevelExportData)item, levelExport);
                    returnList.AddRange(groupedItems);
                }
            }

            return returnList.ToArray();
        }

        public static BackgroundItemSimulatorExportData ConvertBackgroundItem(BackgroundLevelItemExportData levelItem, LevelEditorExportData scene)
        {
            //Schritt 1: Für das LevelItem das zugehörige PhysicItemExportData-Objekt ermitteln
            var protoData = (BackgroundPrototypExportData)scene.Prototyps.PrototypItems.First(x => x.Id == levelItem.PrototypId);
            var image = new Bitmap(protoData.TextureFile); 

            //Schritt 2: Positionsdaten vom LevelItem korrigieren          
            if (levelItem.SizeFactor == 0) levelItem.SizeFactor = 1;
            if (levelItem.LocalPivot == null) levelItem.LocalPivot = new Vec2D(0, 0);

            var initialRecValues = new InitialRotatedRectangleValues()
            {
                SizeFactor = levelItem.SizeFactor,
                AngleInDegree = levelItem.AngleInDegree,
                LocalPivot = levelItem.LocalPivot
            };
            var rotRec = new RotatedRectangle(levelItem.Position, new SizeF(image.Width, image.Height), initialRecValues);

            var cornerPoints = rotRec.GetCornerPoints();

            return new BackgroundItemSimulatorExportData()
            {
                Width = image.Width * levelItem.SizeFactor,
                Height = image.Height * levelItem.SizeFactor,
                AngleInDegree = levelItem.AngleInDegree,
                Center = (cornerPoints[0] + cornerPoints[2]) / 2,
                TextureFile = protoData.TextureFile,
                ZValue = protoData.ZValue,
            };
        }

        public static BackgroundItemSimulatorExportData[] ConvertLawnEdge(LawnEdgeExportData levelItem, LevelEditorExportData scene)
        {
            var polygons = PhysicExportToMergerItemConverter.GetAllPolygonLevelItems(scene);
            var polygon = (LawnEdgePositionCalculator.IPolygon)polygons.First(x => x.LevelItemId == levelItem.PolygonLevelItemId);

            var calc = new LawnEdgePositionCalculator(polygon);
            var p1 = new LawnEdgePositionCalculator.PolygonPoint(levelItem.Index1, levelItem.FPos1, polygon);
            var p2 = new LawnEdgePositionCalculator.PolygonPoint(levelItem.Index2, levelItem.FPos2, polygon);
            return calc.GetAllSegments(p1, p2).Select(segment => new BackgroundItemSimulatorExportData()
            {
                Center = segment.GetCenter(),
                AngleInDegree = segment.GetAngle(),
                Width = segment.GetWidth(),
                Height = levelItem.LawnHeight,
                TextureFile = levelItem.TextureFile,
                ZValue = levelItem.ZValue
            }).ToArray();
        }

        private static BackgroundItemSimulatorExportData[] ConvertGroupedItem(GroupedItemLevelExportData levelItem, LevelEditorExportData scene)
        {
            //Schritt 1: Für das LevelItem das zugehörige GroupedItemProtoExportData-Objekt ermitteln
            var protoData = (GroupedItemProtoExportData)scene.Prototyps.PrototypItems.First(x => x.Id == levelItem.PrototypId);

            //Schritt 2: Alle Kindelemente vom Prototyp erstellen
            var childItems = GetBackgroundItemsFromLevelItems(protoData.LevelItemsExport, scene);

            //Schritt 3: BoundingBox vom protoData ermitteln
            var protoBox = BoundingBoxHelper.GetBoundingBoxFromGroupedItem(protoData, scene);

            //Schritt 3: Positionsdaten vom LevelItem korrigieren                    
            if (levelItem.SizeFactor == 0) levelItem.SizeFactor = 1;
            if (levelItem.LocalPivot == null) levelItem.LocalPivot = new Vec2D(0, 0);

            var initialRecData = new InitialRotatedRectangleValues()
            {
                SizeFactor = levelItem.SizeFactor,
                AngleInDegree = levelItem.AngleInDegree,
                LocalPivot = levelItem.LocalPivot
            };
            var rotRec = new RotatedRectangle(levelItem.Position, protoBox.GetSize(), initialRecData);

            //Schritt 4: LocalToGlobal-Matrix erstellen
            var matrix = Matrix4x4.Translate(-protoBox.Min.X, -protoBox.Min.Y, 0) * rotRec.GetLocalToScreenMatrix();

            return childItems.Select(x => TransformWithMatrix(x, matrix)).ToArray();
        }

        private static BackgroundItemSimulatorExportData TransformWithMatrix(BackgroundItemSimulatorExportData data, Matrix4x4 matrix)
        {
            float angleInDegreeMatrix = Matrix4x4.GetAngleInDegreeFromMatrix(matrix);
            float sizeFactorMatrix = Matrix4x4.GetSizeFactorFromMatrix(matrix);

            return new BackgroundItemSimulatorExportData()
            {
                Center = Matrix4x4.MultPosition(matrix, data.Center),
                AngleInDegree = data.AngleInDegree + angleInDegreeMatrix,
                Width = data.Width * sizeFactorMatrix,
                Height = data.Height * sizeFactorMatrix,
                TextureFile = data.TextureFile,
                ZValue = data.ZValue
            };
        }
    }
}
