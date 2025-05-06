using RigidBodyPhysics.ExportData;
using TextureEditorGlobal;
using TexturePhysicImporter;

namespace TextureEditorControl.Controls.Editor.Model
{
    internal static class ImporterBuilder
    {
        public static IVisualisizerImporter BuildPhysicImporter(PhysicSceneExportData physicSceneData)
        {
            return new PhysicSceneImporter(physicSceneData);
        }
    }
}
