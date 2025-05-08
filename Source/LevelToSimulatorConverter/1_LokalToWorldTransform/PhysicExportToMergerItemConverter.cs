using LevelEditorExports.Editor;
using LevelEditorExports.Editor.LevelItems;
using LevelEditorExports.Editor.Prototyps;
using LevelEditorGlobal.Helper;
using LevelToSimulatorConverter._2_MergeToSingleScene;
using LevelToSimulatorConverter.Helper;
using PhysicGlobal;

namespace LevelToSimulatorConverter._1_LokalToWorldTransform
{
    //Ermittelt für jedes LevelItem dessen LocalToGlobal-Matrix und dessen lokale Positionsdaten
    internal static class PhysicExportToMergerItemConverter
    {
        public static PolygonLevelMergerItem[] GetAllPolygonLevelItems(LevelEditorExportData levelExport)
        {
            List<PolygonLevelMergerItem> returnList = new List<PolygonLevelMergerItem>();

            foreach (var item in levelExport.LevelItems)
            {
                if (item is PolygonLevelItemExportData)
                {
                    var polygonItem = new PolygonLevelMergerItem((PolygonLevelItemExportData)item);
                    returnList.Add(polygonItem);
                }
            }

            PolygonLevelMergerItem.UpdateIsOutsideAndUVFromAllPolygons(returnList);

            return returnList.ToArray();
        }

        

        public static IPhysicMergerItem[] GetPhysicLevelItemsFromExportScene(LevelEditorExportData levelExport)
        {
            List<IPhysicMergerItem> returnList = new List<IPhysicMergerItem>();

            List<PolygonLevelMergerItem> polygons = new List<PolygonLevelMergerItem>();

            foreach (var item in levelExport.LevelItems)
            {
                if (item is PhysicLevelItemExportData)
                {
                    returnList.Add(ConvertPhysicLevelItem((PhysicLevelItemExportData)item, levelExport));
                }

                if (item is PolygonLevelItemExportData)
                {
                    var polygonItem = new PolygonLevelMergerItem((PolygonLevelItemExportData)item);
                    returnList.Add(polygonItem);
                    polygons.Add(polygonItem);
                }

                if (item is GroupedItemLevelExportData)
                {
                    returnList.AddRange(ConvertGroupedLevelItem((GroupedItemLevelExportData)item, levelExport));
                }
            }

            PolygonLevelMergerItem.UpdateIsOutsideAndUVFromAllPolygons(polygons);

            return returnList.ToArray();
        }

        public static PhysicLevelMergerItem ConvertPhysicLevelItem(PhysicLevelItemExportData levelItem, LevelEditorExportData scene)
        {
            //Schritt 1: Für das LevelItem das zugehörige PhysicItemExportData-Objekt ermitteln
            var protoData = (PhysicItemExportData)scene.Prototyps.PrototypItems.First(x => x.Id == levelItem.PrototypId);
            if (protoData.InitialRecValues == null) protoData.InitialRecValues = new InitialRotatedRectangleValues();

            //Schritt 2: BoundingBox vom protoData ermitteln
            var protoPhysicScene = protoData.PhysicSceneData;
            if (protoData.PhysicSceneForAnimationNull != null)
                protoPhysicScene = protoData.PhysicSceneForAnimationNull;
            var protoBox = PhysicSceneExportDataHelper.GetBoundingBoxFromScene(protoPhysicScene);

            //Schritt 3: Positionsdaten vom LevelItem korrigieren                    
            if (levelItem.SizeFactor == 0) levelItem.SizeFactor = 1;
            if (levelItem.LocalPivot == null) levelItem.LocalPivot = new Vec2D(0, 0);
            var initialRecValues = new InitialRotatedRectangleValues()
            {
                SizeFactor = levelItem.SizeFactor,
                AngleInDegree = levelItem.AngleInDegree,
                LocalPivot = levelItem.LocalPivot
            };
            var rotRec = new RotatedRectangle(levelItem.Position, protoBox.GetSize(), initialRecValues);

            //Schritt 4: LocalToGlobal-Matrix erstellen
            var m1 = Matrix4x4.Translate(-protoBox.Min.X, -protoBox.Min.Y, 0);
            var m2 = rotRec.GetLocalToScreenMatrix();

            return new PhysicLevelMergerItem(levelItem.LevelItemId, protoData, m1 * m2);
        }

        public static PhysicLevelMergerItem[] ConvertGroupedLevelItem(GroupedItemLevelExportData levelItem, LevelEditorExportData scene)
        {
            //Schritt 1: Für das LevelItem das zugehörige GroupedItemProtoExportData-Objekt ermitteln
            var protoData = (GroupedItemProtoExportData)scene.Prototyps.PrototypItems.First(x => x.Id == levelItem.PrototypId);
            
            //Schritt 2: Alle Kindelemente vom Prototyp erstellen
            List<IMergeablePhysicScene> childItems = new List<IMergeablePhysicScene>();
            foreach (var item in protoData.LevelItemsExport)
            {
                if (item is PhysicLevelItemExportData)
                {
                    childItems.Add(ConvertPhysicLevelItem((PhysicLevelItemExportData)item, scene));
                }

                if (item is GroupedItemLevelExportData)
                {
                    childItems.AddRange(ConvertGroupedLevelItem((GroupedItemLevelExportData)item, scene));
                }
            }

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
            var groupedItemMatrix = Matrix4x4.Translate(-protoBox.Min.X, -protoBox.Min.Y, 0) * rotRec.GetLocalToScreenMatrix();

            //Schritt 5: Die Kindelemente mit der LocalToGlobal-Matrix multiplizieren
            return childItems.Select(x => new PhysicLevelMergerItem(-1, x.PhysicData, x.GetTranslationMatrix() * groupedItemMatrix)).ToArray();
        }

        
    }


    internal class PhysicLevelMergerItem : IMergeablePhysicScene
    {
        public int LevelItemId { get; }
        public Matrix4x4 GetTranslationMatrix() => this.translationMatrix; //Enthält die Positon/Ausrichtung/Skalierung vom LevelItem
        public PhysicItemExportData PhysicData { get; }

        private Matrix4x4 translationMatrix;

        public PhysicLevelMergerItem(int levelItemId, PhysicItemExportData physicData, Matrix4x4 translationMatrix)
        {
            LevelItemId = levelItemId;
            PhysicData = physicData;
            this.translationMatrix = translationMatrix;
        }
    }

    internal class PolygonLevelMergerItem : IMergeablePhysicPolygon, LawnEdgePositionCalculator.IPolygon
    {
        public int LevelItemId { get; }
        public Vec2D[] Points { get; }
        public bool IsOutside { get; private set; } //Zeigen die Normalen nach Außen?
        public int ZOrder { get; private set; }
        public float Friction { get; }
        public float Restiution { get; }
        public int CollisionCategory { get; }

        public PolygonLevelMergerItem(PolygonLevelItemExportData levelItem)
        {
            LevelItemId = levelItem.LevelItemId;
            Points = levelItem.Points;
            IsOutside = true;
            ZOrder = 0;
            Friction = levelItem.Friction;
            Restiution = levelItem.Restiution;
            CollisionCategory = levelItem.CollisionCategory;
        }

        public static void UpdateIsOutsideAndUVFromAllPolygons(List<PolygonLevelMergerItem> polygons)
        {
            PolygonHelper.UpdateIsOutsideAndUVFromAllPolygons<PolygonLevelMergerItem>(
                polygons.Select(x => new PolygonHelper.NestedPolygon<PolygonLevelMergerItem>(x.Points, x)).ToArray(),
                (poly, zOrder, isOutside) =>
                {
                    poly.ZOrder = zOrder;
                    poly.IsOutside = isOutside;
                }
                );
        }
    }
}
