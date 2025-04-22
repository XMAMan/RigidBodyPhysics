namespace KeyFramePhysicImporter.Model.PhysicSceneDrawing
{
    internal static class ColorExtensions
    {
        public static Color ToDrawingColor(this System.Windows.Media.SolidColorBrush br)
        {
            return Color.FromArgb(
                br.Color.A,
                br.Color.R,
                br.Color.G,
                br.Color.B);
        }
    }
}
