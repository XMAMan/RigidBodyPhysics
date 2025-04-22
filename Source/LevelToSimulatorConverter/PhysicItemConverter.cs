using DynamicData;
using GraphicMinimal;
using KeyFrameGlobal;
using LevelEditorGlobal;
using PhysicItemEditorControl.Model;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System.Drawing;
using TextureEditorControl;
using TextureEditorGlobal;

namespace LevelToSimulatorConverter
{
    public interface IPhysicMergerItem
    {
        int LevelItemId { get; }
    }

    public interface IMergeablePhysicScene : IPhysicMergerItem
    {
        Matrix4x4 GetTranslationMatrix(); //Enthält die Positon/Ausrichtung/Skalierung vom LevelItem
        object PhysicData { get; }
    }

    public interface IMergeablePhysicPolygon : IPhysicMergerItem
    {
        Vector2D[] Points { get; }
        bool IsOutside { get; } //Zeigen die Normalen nach Außen?
        int ZOrder { get; }
        float Friction { get; }
        float Restiution { get; }
        int CollisionCategory { get; }
    }

    //Konvertiert IPhysicMergerItem-Objekte welche vom Editor kommen in PhysikLevelItemExportData-Objekte 
    internal static class PhysicItemConverter
    {
        //Konvertiert ein PhysicLevelItem in ein PhysicSceneExportData
        private static PhysicSceneExportData GetPhysicDataFromPhysicItem(IMergeablePhysicScene physicItem)
        {
            var protoData = (PhysicItemExportData)physicItem.PhysicData;

            //Schritt 1: Merge das PhysicScene-Objekt
            var subScene = protoData.PhysicSceneData;

            //Wenn vorhanden nimm die PhysicScene welche bei PlayPosition=0 ist um ein Sprung bei Simulationsstart zu vermeiden
            if (protoData.PhysicSceneForAnimationNull != null)
            {
                subScene = protoData.PhysicSceneForAnimationNull;
            }

            subScene = JsonHelper.Helper.FromCompactJson<PhysicSceneExportData>(JsonHelper.Helper.ToJson(subScene)); //Erstelle Kopie damit die Originaldaten nicht zerstört werden

            //Die CollisionCategory kann vom Editor beim physicItem.PhysicData bearbeitet worden sein.
            //Wenn man dann nicht nochmal im Editor von den PhysicObjekt war, dann wird die CollisionCategory 
            //nicht auf das protoData.PhysicSceneForAnimationNull übertragen. Deswegen mache ich das hier noch nachträglich
            if (protoData.PhysicSceneForAnimationNull != null)
            {
                for (int j = 0; j < subScene.Bodies.Length; j++)
                {
                    subScene.Bodies[j].CollisionCategory = protoData.PhysicSceneData.Bodies[j].CollisionCategory;
                }
            }

            PhysicSceneExportDataHelper.TranslateScene(subScene, physicItem.GetTranslationMatrix());

            return subScene;
        }

        

        //Konvertiert ein PolygonLevelItem in ein PhysicSceneExportData
        private static PhysicSceneExportData GetPhysicDataFromPolygon(IMergeablePhysicPolygon physicPolygon, int maxZOrder)
        {
            var center = PolygonHelper.GetCenterOfMassFromPolygon(physicPolygon.Points.Select(x => x.ToPhx()).ToArray());
            var pointsLocal = physicPolygon.Points.Select(x => x.ToPhx() - center).ToArray();

            var polygonType = physicPolygon.IsOutside ? PolygonCollisionType.EdgeWithNormalsPointingOutside : PolygonCollisionType.EdgeWithNormalsPointingInside;
            if (physicPolygon.ZOrder == maxZOrder && physicPolygon.IsOutside == true) polygonType = PolygonCollisionType.Rigid; //Das innerste Polygon darf Rigid sein (dort kann die Kollisionsabfrage auch Objekte rausdrücken, die komplett innerhalb vom Polygon liegen)

            var polygon = new PolygonExportData()
            {
                PolygonType = polygonType,
                Points = pointsLocal,
                Center = center,
                AngleInDegree = 0,
                Velocity = new Vec2D(0, 0),
                AngularVelocity = 0,
                MassData = new MassData(MassData.MassType.Infinity, 1, 1),
                Friction = physicPolygon.Friction,
                Restituion = physicPolygon.Restiution,
                CollisionCategory = physicPolygon.CollisionCategory,
            };
            var polygonScene = new PhysicSceneExportData() { Bodies = new IExportRigidBody[] { polygon } };

            return polygonScene;
        }

        //Konvertiert die Texturdaten von ein PhysicLevelItem
        private static VisualisizerOutputData GetTextureDataFromPhysicItem(IMergeablePhysicScene physicItem, PhysicSceneExportData subScene)
        {
            var protoData = (PhysicItemExportData)physicItem.PhysicData;

            VisualisizerOutputData textureData = null;
            if (protoData.TextureData != null)
            {
                textureData = new VisualisizerOutputData(protoData.TextureData);
            }
            else
            {
                textureData = TextureEditorFactory.CreateDefaultTextureData(subScene);
            }

            float sizeFactor = Matrix4x4.GetSizeFactorFromMatrix(physicItem.GetTranslationMatrix());
            foreach (var tex in textureData.Textures)
            {
                tex.Width = tex.Width * sizeFactor;
                tex.Height = tex.Height * sizeFactor;
                tex.DeltaX = tex.DeltaX * sizeFactor;
                tex.DeltaY = tex.DeltaY * sizeFactor;
            }

            return textureData;
        }

        //Konvertiet die Texturdaten von ein PolygonLevelItem
        private static TextureExportData GetTextureDataFromPolygon(IMergeablePhysicPolygon physicPolygon, RigidBodyPhysics.MathHelper.BoundingBox polyBox, string backgroundImage, string foregroundImage)
        {
            //Alle Textur-Rechtecke von allen Polygonen sind genau gleich groß und liegen an der gleiche Stelle
            //Damit sie an der gleichen Stelle liegen müssen sie per Delta zum globalen Zentrum verschoben werden
            Vec2D min = new Vec2D(physicPolygon.Points.Min(x => x.X), physicPolygon.Points.Min(x => x.Y));
            Vec2D max = new Vec2D(physicPolygon.Points.Max(x => x.X), physicPolygon.Points.Max(x => x.Y));
            Vec2D texCenter = min + (max - min) / 2; //Hier ist das Zentrum der Textur, wo es hin kommen würde, wenn DeltaXY=0 ist
            Vec2D globalBoxCenter = polyBox.Center; //Hier ist das Zentrum von allen Polygonen. 
            Vec2D delta = globalBoxCenter - texCenter; //Damit die Textur von diesen Polygon am GlobalCenter liegt muss es um Delta verschoben werden

            string textureFile = physicPolygon.IsOutside ? foregroundImage : backgroundImage;

            var texture = new TextureExportData()
            {
                TextureFile = textureFile,
                MakeFirstPixelTransparent = false,
                ColorFactor = Color.White,
                DeltaX = (int)delta.X,
                DeltaY = (int)delta.Y,
                Width = (int)polyBox.Width,
                Height = (int)polyBox.Height,
                DeltaAngle = 0,
                ZValue = physicPolygon.ZOrder,
                IsInvisible = string.IsNullOrEmpty(textureFile)
            };

            return texture;
        }

        //Konvertiert jedes IPhysicMergerItem-Objekt jeweils in ein PhysicSceneExportData
        private static PhysicSceneExportData[] GetPhysicData(List<IPhysicMergerItem> items)
        {
            List<PhysicSceneExportData> returnList = new List<PhysicSceneExportData>();

            int maxZOrder = -1;
            if (items.Any(x => x is IMergeablePhysicPolygon))
            {
                maxZOrder = items
                .Where(x => x is IMergeablePhysicPolygon)
                .Cast<IMergeablePhysicPolygon>()
                .Select(x => x.ZOrder)
                .Max();
            }

            foreach (var item in items)
            {
                if (item is IMergeablePhysicScene)
                {
                    returnList.Add(GetPhysicDataFromPhysicItem((IMergeablePhysicScene)item));
                }

                if (item is IMergeablePhysicPolygon)
                {
                    returnList.Add(GetPhysicDataFromPolygon((IMergeablePhysicPolygon)item, maxZOrder));
                }
            }

            return returnList.ToArray();
        }

        //Konvertiert jedes IPhysicMergerItem-Objekt jeweils in ein VisualisizerOutputData
        private static VisualisizerOutputData[] GetTextureData(List<IPhysicMergerItem> items, PhysicSceneExportData[] physicData, string backgroundImage, string foregroundImage)
        {
            List<VisualisizerOutputData> returnList = new List<VisualisizerOutputData>();

            var polyBox = RigidBodyPhysics.MathHelper.BoundingBox.GetBoxFromBoxes(items
                .Where(x => x is IMergeablePhysicPolygon)
                .Cast<IMergeablePhysicPolygon>()
                .Select(x => PolygonHelper.GetBoundingBoxFromPolygon(x.Points.Select(y => y.ToPhx()).ToArray()))
                );

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is IMergeablePhysicScene)
                {
                    returnList.Add(GetTextureDataFromPhysicItem((IMergeablePhysicScene)item, physicData[i]));
                }

                if (item is IMergeablePhysicPolygon)
                {
                    var texture = GetTextureDataFromPolygon((IMergeablePhysicPolygon)item, polyBox, backgroundImage, foregroundImage);
                    returnList.Add(new VisualisizerOutputData(new TextureExportData[] { texture }));
                }
            }

            return returnList.ToArray();
        }

        //Gibt von allen LevelItems ihre Animationsdaten zurück
        private static List<AnimationOutputData[]> GetAnimationData(List<IPhysicMergerItem> items)
        {
            List<AnimationOutputData[]> returnList = new List<AnimationOutputData[]>();

            foreach (var item in items)
            {
                if (item is IMergeablePhysicScene)
                {
                    var physicItem = (IMergeablePhysicScene)item;
                    var protoData = (PhysicItemExportData)physicItem.PhysicData;
                    if (protoData.AnimationData != null)
                    {
                        var animations = protoData.AnimationData.Select(x => x.AnimationData).ToArray();
                        foreach (var animation in animations)
                        {
                            //Korrigiere die Eingangsdaten
                            animation.ReplaceDoublesWithFloats(); //Newtonsoft.JSON wandelt floats in doubles um. Das korrigiere ich hier
                        }
                        returnList.Add(animations);
                    }else
                    {
                        returnList.Add(null);
                    }
                }else
                {
                    returnList.Add(null);
                }
            }

            return returnList;
        }

        //Gibt von allen LevelItems ihre Keyboard-Daten zurück
        private static List<KeyboardMappingEntry[]> GetKeyboardData(List<IPhysicMergerItem> items, KeyboardMappingTable[] keyboardMappings)
        {
            List<KeyboardMappingEntry[]> returnList = new List<KeyboardMappingEntry[]>();
            foreach (var item in items)
            {
                var entries = keyboardMappings.Where(x => x.LevelItemId == item.LevelItemId).SelectMany(x => x.Entries).ToArray();
                returnList.Add(entries);
            }
            return returnList;
        }

        //Gibt von allen LevelItems ihre Tag-Daten zurück
        private static List<PhysicSceneTagdataEntry[]> GetTagData(List<IPhysicMergerItem> items, EditorTagdata[] allEditorTags)
        {
            List<PhysicSceneTagdataEntry[]> returnList = new List<PhysicSceneTagdataEntry[]>();
            foreach (var levelItem in items)
            {
                var levelItemTags = allEditorTags
                    .Where(x => x.LevelItemId == levelItem.LevelItemId)
                    .Select(x => ConvertEditorToSimulatorTag(x, levelItem))
                    .ToArray();

                returnList.Add(levelItemTags);
            }
            return returnList;
        }
        private static PhysicSceneTagdataEntry ConvertEditorToSimulatorTag(EditorTagdata tag, IPhysicMergerItem levelItem)
        {
            //Part 1: TagNames
            List<string> names = new List<string>();

            if (string.IsNullOrEmpty(tag.PrototypTagName) == false)
                names.Add(tag.PrototypTagName);

            if (string.IsNullOrEmpty(tag.Name) == false)
                names.Add(tag.Name);

            //Part 2: TagColor
            byte color = tag.PrototypColor;
            if (tag.Color != 0)
                color = tag.Color;

            //Part 3: AnchorPoints
            float levelItemSizeFactor = 1;
            if (levelItem is IMergeablePhysicScene)
            {
                var physicItem = (IMergeablePhysicScene)levelItem;
                levelItemSizeFactor = Matrix4x4.GetSizeFactorFromMatrix(physicItem.GetTranslationMatrix());
            }
            return new PhysicSceneTagdataEntry(tag.TagType, tag.LevelItemId, tag.TagId, names.ToArray(), color, tag.AnchorPoints.Select(x => x * levelItemSizeFactor).ToArray());
        }
        
        internal static PhysikLevelItemExportData[] Convert(List<IPhysicMergerItem> items, string backgroundImage, string foregroundImage, KeyboardMappingTable[] keyboardMappings, EditorTagdata[] tags)
        {
            List<PhysikLevelItemExportData> returnList = new List<PhysikLevelItemExportData>();

            var physicData = GetPhysicData(items);
            var textureData = GetTextureData(items, physicData, backgroundImage, foregroundImage);
            var animationData = GetAnimationData(items);
            var keyboardData = GetKeyboardData(items, keyboardMappings);
            var tagData = GetTagData(items, tags);

            for (int i=0;i<items.Count;i++)
            {
                returnList.Add(new PhysikLevelItemExportData()
                {
                    LevelItemId = items[i].LevelItemId,
                    PhysicSceneData = physicData[i],
                    TextureData = textureData[i],
                    AnimationData = animationData[i],
                    KeyboardMappings = keyboardData[i],
                    TagdataEntries = tagData[i]
                });
            }

            return returnList.ToArray();
        }
    }
}
