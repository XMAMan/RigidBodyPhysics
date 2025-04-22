using KeyFrameEditorControl.Controls.KeyFrameEditor;
using LevelEditorGlobal;
using RigidBodyPhysics.ExportData;
using TextureEditorGlobal;

namespace PhysicItemEditorControl.Model
{
    public class PhysicItemExportData : IPrototypExportData
    {
        public IPrototypItem.Type ProtoType => IPrototypItem.Type.PhysicItem;
        public int Id { get; set; }
        public PhysicSceneExportData PhysicSceneData { get; set; } //So hat es der PhysicEditor erzeugt
        public PhysicSceneExportData PhysicSceneForAnimationNull { get; set; } //So sieht die Scene bei PlayPosition=0 aus
        public VisualisizerOutputData TextureData { get; set; }
        public KeyFrameEditorExportData[] AnimationData { get; set; }
        public InitialRotatedRectangleValues InitialRecValues { get; set; }
        public int[] CameraTrackedRigidBodyIds { get; set; } //Wird vom PhysicSceneMerger erzeugt. Das PhysicItemViewModel läßt diesen Wert leer

    }
}
