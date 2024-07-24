using GraphicMinimal;
using System.Drawing;

namespace TextureEditorGlobal
{
    public class VisualisizerInputData
    {
        public I2DAreaShape[] Shapes { get; set; }
    }

    //2D-Figur die eine Fläche einnimmt (Kein Kreis/Linie/Punkt)
    //Diese Figur wurde im Lokalspace definiert und dann zum Punkt "Center" hin verschoben und dort dann noch um "Angle" gedreht
    public interface I2DAreaShape
    {
        Vector2D Center { get; } //Um diesen Punkt wird das Objekt was texturiert werden soll gedreht (Entspricht dem Massezentrum)
        float AngleInDegree { get; }

        //AABB-BoundingBox von der Shape im LokalSpace (Wenn Objekt nicht gedreht wurde und dessen Zentrum bei 0 liegt)
        RectangleF LocalBoundingBox { get; }
    }

    public interface IRectangle : I2DAreaShape
    {
        float Width { get; }
        float Height { get; }
    }

    public interface IPolygon : I2DAreaShape
    {
        Vector2D[] Points { get; }
    }

    public interface ICircle : I2DAreaShape
    {
        float Radius { get; }
    }
}
