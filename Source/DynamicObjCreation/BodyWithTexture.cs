using RigidBodyPhysics.ExportData.RigidBody;
using TextureEditorGlobal;

namespace DynamicObjCreation
{
    //Diese Klasse hilft, wenn man Astroide/Steine/Bruchstücke von Gegenständen erzeugen will
    public class BodyWithTexture
    {
        public IExportRigidBody Body { get; }
        public TextureExportData Texture { get; }
        public byte TagColor { get; set; }
        public string[] TagNames { get; set; }

        public BodyWithTexture(IExportRigidBody body, TextureExportData texture, byte tagColor, string[] tagNames)
        {
            Body = body;
            Texture = texture;
            TagColor = tagColor;
            TagNames = tagNames;
        }
    }
}
