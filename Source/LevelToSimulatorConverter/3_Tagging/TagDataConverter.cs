using LevelEditorExports.Editor.LevelItems;
using LevelEditorExports.Editor.Prototyps;
using LevelEditorExports.Editor.Tagging;
using LevelEditorExports.Simulator;
using LevelEditorGlobal;
using LevelEditorGlobal.Helper;

namespace LevelToSimulatorConverter._3_Tagging
{
    internal static class TagDataConverter
    {
        public static EditorTagdata[] Convert(TagEditorDataExport tagData, PrototypControlExportData prototyps, ILevelItemExportData[] levelItems)
        {
            List < EditorTagdata > returnList = new List<EditorTagdata>();

            foreach (var levelItem in levelItems)
            {
                if (levelItem is PhysicLevelItemExportData)
                {
                    var physicItem = (PhysicLevelItemExportData)levelItem;

                    var protoData = (PhysicItemExportData)prototyps.PrototypItems.First(x => x.Id == physicItem.PrototypId);

                    string protoTagName = TreeItemNameCreator.CreateNameForProtoItem(physicItem.PrototypId);
                    var protoTagData = tagData.Tags.FirstOrDefault(x => x.Id == protoTagName);
                    if (protoTagData == null) protoTagData = new TagEditorData(protoTagName);

                    returnList.AddRange(GetEditorTagdatas(tagData, TagType.Body, protoData.PhysicSceneData.Bodies.Length, levelItem.LevelItemId, protoTagData));
                    returnList.AddRange(GetEditorTagdatas(tagData, TagType.Joint, protoData.PhysicSceneData.Joints.Length, levelItem.LevelItemId, protoTagData));
                    returnList.AddRange(GetEditorTagdatas(tagData, TagType.Thruster, protoData.PhysicSceneData.Thrusters.Length, levelItem.LevelItemId, protoTagData));
                    returnList.AddRange(GetEditorTagdatas(tagData, TagType.Motor, protoData.PhysicSceneData.Motors.Length, levelItem.LevelItemId, protoTagData));
                    returnList.AddRange(GetEditorTagdatas(tagData, TagType.AxialFriction, protoData.PhysicSceneData.AxialFrictions.Length, levelItem.LevelItemId, protoTagData));

                }

                if (levelItem is PolygonLevelItemExportData)
                {
                    var polygonItem = (PolygonLevelItemExportData)levelItem;

                    string levelItemTagName = TreeItemNameCreator.CreateNameForLevelItem(levelItem.LevelItemId);

                    var bodyTagData = tagData.Tags.FirstOrDefault(x => x.Id == levelItemTagName);
                    if (bodyTagData == null) bodyTagData = new TagEditorData(levelItemTagName);

                    var data = new EditorTagdata()
                    {
                        LevelItemId = polygonItem.LevelItemId,
                        TagId = 0,
                        TagType = TagType.Polygon,
                        PrototypTagName = null,
                        PrototypColor = 0,
                        Name = bodyTagData.Name,
                        Color = bodyTagData.Color,
                        AnchorPoints = bodyTagData.AnchorPoints.ToArray()
                    };

                    returnList.Add(data);



                }
            }

            return returnList.ToArray();
        }

        private static List<EditorTagdata> GetEditorTagdatas(TagEditorDataExport tagData, TagType tagType, int count, int levelItemId, TagEditorData protoTagData)
        {
            List<EditorTagdata> returnList = new List<EditorTagdata>();
            for (int index = 0; index < count; index++)
            {
                string tagName = TreeItemNameCreator.CreateNameForRigidBodyItem(tagType, levelItemId, index);
                var bodyTagData = tagData.Tags.FirstOrDefault(x => x.Id == tagName);
                if (bodyTagData == null) bodyTagData = new TagEditorData(tagName);

                var data = new EditorTagdata()
                {
                    LevelItemId = levelItemId,
                    TagId = index,
                    TagType = tagType,
                    PrototypTagName = protoTagData.Name,
                    PrototypColor = protoTagData.Color,
                    Name = bodyTagData.Name,
                    Color = bodyTagData.Color,
                    AnchorPoints = bodyTagData.AnchorPoints.ToArray()
                };

                returnList.Add(data);
            }

            return returnList;
        }

    }

    internal interface ITagContainer
    {
        ITagable[] Tagables { get; }
    }
}
