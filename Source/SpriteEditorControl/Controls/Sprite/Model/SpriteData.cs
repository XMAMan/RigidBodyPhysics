using System.Drawing;

namespace SpriteEditorControl.Controls.Sprite.Model
{
    public class SpriteData
    {
        public Bitmap Image { get; set; } //Sprite-Bild was aus Boxes.Length Einzelbildern innerhalb einer Zeile besteht
                                          //Jedes Einzelbild ist Image.Width / Boxes.Length breit und Image.Height groß
        public Rectangle[] Boxes { get; set; } //Innerhalb von jeden Einzelbild befinden sich die Daten dann in Boxes
    }
}
