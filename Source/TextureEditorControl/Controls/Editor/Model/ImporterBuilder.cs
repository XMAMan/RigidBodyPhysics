using TextureEditorGlobal;
using TexturePhysicImporter;

namespace TextureEditorControl.Controls.Editor.Model
{
    internal static class ImporterBuilder
    {
        public static IVisualisizerImporter BuildPhysicImporter(string physicSceneJson)
        {
            return new PhysicSceneImporter(physicSceneJson);
        }
    }
}
