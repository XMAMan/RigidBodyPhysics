using GraphicMinimal;

namespace TextureEditorGlobal
{
    //Erzeugt die Eckpunkte und das Zentrum von einer BoundingBox, welche ein Starrkörper umschließt, der gedreht wurde 
    public static class TextureRectangleHelper
    {
        //Gibt die 4 Eckpuntke von der rotierten BoundBox eines Starrkörpers zurück und als 5. Punkt das Zentrum dieser rotierten Box
        //bodyCenter = Masseschwerpunkt vom Starrkörper
        //localAABBCenter = AABBox von der Körper, wenn Angle=0 und Center=(0,0)
        //width/height = Breite/Höhe von der AABBox des Körper, wenn Angle=0 und Center=(0,0)
        //angleInDegree = Ausrichtung des Starrkörper
        //deltaX/deltaY = Um diesen Vektor wird die BoundingBox im unrotierten Körper verschoben
        //deltaAngle = Um diesen Winkel werden die Eckpunkte der BoundingBox im unrotierten Körper gedreht
        public static Vector2D[] GetTextureBorderPoints(Vector2D bodyCenter, Vector2D localAABBCenter, float width, float height, float angleInDegree, float deltaX, float deltaY, float deltaAngle)
        {
            //Schritt 1: Im Lokalspace das Zentrum um DeltaXY verschieben und dann um angleInDegree drehen
            var localCenter = bodyCenter + localAABBCenter;
            var texCenter = new Vector2D(localCenter.X, localCenter.Y) + new Vector2D(deltaX, deltaY); //Richtungsvektor vom Masseschwerpunkt zum Textur-Rechteck-Zentrum

            //Schritt 2: An der texCenter-Position das TexturRechteck erstellen
            Vector2D[] local = new Vector2D[]
            {
                new Vector2D(texCenter.X - width / 2, texCenter.Y - height / 2),
                new Vector2D(texCenter.X + width / 2, texCenter.Y - height / 2),
                new Vector2D(texCenter.X + width / 2, texCenter.Y + height / 2),
                new Vector2D(texCenter.X - width / 2, texCenter.Y + height / 2)
            };

            //Schritt 3: Drehe um Angle und DeltaAngle
            var points = local.Select(x => Vector2D.RotatePointAroundPivotPoint(texCenter, x, deltaAngle)).ToList();
            points = points.Select(x => Vector2D.RotatePointAroundPivotPoint(bodyCenter, x, angleInDegree)).ToList();

            points.Add((points[0] + points[2]) / 2); //Textur-Zentrum

            return points.ToArray();
        }
    }
}
