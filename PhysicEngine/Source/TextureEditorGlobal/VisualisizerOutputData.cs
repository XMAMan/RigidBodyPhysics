using System.Drawing;

namespace TextureEditorGlobal
{
    public class VisualisizerOutputData
    {
        public TextureExportData[] Textures { get; set; }

        public VisualisizerOutputData() { }

        public VisualisizerOutputData(TextureExportData[] textures) 
        { 
            this.Textures = textures;
        }

        public VisualisizerOutputData(VisualisizerOutputData copy)
        {
            this.Textures = new TextureExportData[copy.Textures.Length];
            for (int i = 0; i < copy.Textures.Length; i++)
                this.Textures[i] = new TextureExportData(copy.Textures[i]);
        }
    }

    //Es soll eine 2D-Shape die ein Zentrum und eine Ausrichtung (Angle) (Drehung um die Z-Achse) hat texturiert wird.
    //Dazu wird um die Shape im Lokalspace (Angle ist 0) eine axiale BoundingBox erstellt. Diese Lokal-Space-AABB wird um Angle
    //gedreht. DeltaX zeigt im LokalSpace nach (1,0) und dann wird dieser Vektor um Angle gedreht. Nun gehe ich vom Zentrum
    //in die gedrehte DeltaX-Richtung um DeltaX Schritte. Das gleiche mache ich für DeltaY. Nun zeichne ich ein Rechteck an
    //den Delta-Versetzen Zentrum mit Größe Width/Height. Ich drehe dieses Rechteck dann aber nochmal um DeltaAngle.
    public class TextureExportData
    {
        public string TextureFile { get; set; }
        public bool MakeFirstPixelTransparent { get; set; }
        public Color ColorFactor { get; set; }
        public float DeltaX { get; set; }
        public float DeltaY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float DeltaAngle { get; set; }
        public float ZValue { get; set; }
        public bool IsInvisible { get; set; }

        public TextureExportData() { }

        public TextureExportData(TextureExportData copy)
        {
            this.TextureFile = copy.TextureFile;
            this.MakeFirstPixelTransparent = copy.MakeFirstPixelTransparent;
            this.ColorFactor = copy.ColorFactor;
            this.DeltaX = copy.DeltaX;
            this.DeltaY = copy.DeltaY;
            this.Width = copy.Width;
            this.Height = copy.Height;
            this.DeltaAngle = copy.DeltaAngle;
            this.ZValue = copy.ZValue;
            this.IsInvisible = copy.IsInvisible;
        }
    }
}
