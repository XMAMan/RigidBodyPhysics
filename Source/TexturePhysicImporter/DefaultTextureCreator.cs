using RigidBodyPhysics.ExportData;
using System.Drawing;
using TextureEditorGlobal;

namespace TexturePhysicImporter
{
    public static class DefaultTextureCreator
    {
        public static VisualisizerOutputData CreateDefaultTextureData(PhysicSceneExportData physicSceneData)
        {
            var animationInputData = new PhysicSceneImporter(physicSceneData).Import();
            return new VisualisizerOutputData(animationInputData.Shapes.Select(x => GetDefaultTexture(GetSizeFromShape(x))).ToArray());
        }

        private static Size GetSizeFromShape(I2DAreaShape shape)
        {
            return new Size((int)shape.LocalBoundingBox.Width, (int)shape.LocalBoundingBox.Height);
        }

        private static TextureExportData GetDefaultTexture(Size size)
        {
            return new TextureExportData()
            {
                TextureFile = "",
                MakeFirstPixelTransparent = true,
                ColorFactor = Color.FromArgb(255, 255, 255),
                DeltaX = 0,
                DeltaY = 0,
                Width = size.Width,
                Height = size.Height,
                DeltaAngle = 0,
                ZValue = 0,
                IsInvisible = false,
            };
        }
    }
}
