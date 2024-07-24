using KeyFrameGlobal;
using RigidBodyPhysics.ExportData;
using TextureEditorGlobal;

namespace LevelEditorGlobal
{
    //Speichert alle ExportDaten von LevelItems, welche Bestandteil der PhysicScene sind
    public class PhysikLevelItemExportData
    {
        public int LevelItemId;
        public PhysicSceneExportData PhysicSceneData; //Ohne CollisionMatrix
        public VisualisizerOutputData TextureData;
        public AnimationOutputData[] AnimationData;
        public KeyboardMappingEntry[] KeyboardMappings;
        public PhysicSceneTagdataEntry[] TagdataEntries;

        public PhysikLevelItemExportData() { }  

        public PhysikLevelItemExportData(PhysikLevelItemExportData copy)
        {
            this.LevelItemId = copy.LevelItemId;
            this.PhysicSceneData = new PhysicSceneExportData(copy.PhysicSceneData);
            this.TextureData = new VisualisizerOutputData(copy.TextureData);
            this.AnimationData = copy.AnimationData.Select(x => new  AnimationOutputData(x)).ToArray();
            this.KeyboardMappings = copy.KeyboardMappings.Select(x => new KeyboardMappingEntry(x)).ToArray();
            this.TagdataEntries = copy.TagdataEntries.Select(x => new PhysicSceneTagdataEntry(x)).ToArray();
        }
    }
}
