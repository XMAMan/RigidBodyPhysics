using LevelEditorExports.Simulator;

namespace LevelEditorExports.Editor.Helper
{
    //Die hier verwendeten Namen werden beim Exportieren der Tagdaten bei der TagEditorData.Id-Property verwendet
    //Damit sowohl der Editor als auch der Simulator beim Einlesen der Editordaten die gleichen Namen verwenden, werden sie hier definiert
    public static class TreeItemNameCreator
    {
        public static string CreateNameForProtoItem(int protoItemId)
        {
            return "Proto_" + protoItemId;
        }

        public static string CreateNameForLevelItem(int levelItemId)
        {
            return "Level_" + levelItemId;
        }

        public static string CreateNameForRigidBodyItem(TagType tagType,  int levelItemId, int index)
        {
            return tagType + "_" + levelItemId + "_" + index;
        }
    }
}
