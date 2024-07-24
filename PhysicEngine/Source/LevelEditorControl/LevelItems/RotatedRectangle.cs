using GraphicMinimal;
using LevelEditorControl.EditorFunctions;
using LevelEditorGlobal;
using System.Drawing;
using System.Linq;

namespace LevelEditorControl.LevelItems
{
    //Das ist ein Rechteck, was im Original-Zustand OriginalSize groß ist und was dann aber noch um SizeFactor skaliert wurde
    //und um AngleInDegree gedreht wurde. Der Drehpunkt ist der PivotPoint. Dieser Punkt liegt aber nicht zwangsweise im Rechteck-Zentrum.
    internal class RotatedRectangle
    {
        public Vector2D PivotPoint { get; set; } //Das ist die Mausklickposition aus der AddItem/MoveSelect-Funktion
        public SizeF OriginalSize { get; set; } //Breite und höhe des Objektes wenn es nicht gedreht/skaliert wurde
        public Vector2D LocalPivot { get; set; } //Vektor der von der linken oberen Ecke zum Pivot-Punkt zeigt (0,0) = Linke obere Ecke; (1,0) = Rechte obere Ecke; (1,1) = Reche untere Ecke
        public float SizeFactor { get; set; }
        public float AngleInDegree { get; set; }

        public RotatedRectangle(Vector2D pivotPoint, SizeF originalSize, InitialRotatedRectangleValues initialRecValues)
        {
            PivotPoint = pivotPoint;
            OriginalSize = originalSize;
            LocalPivot = initialRecValues.LocalPivot;
            SizeFactor = initialRecValues.SizeFactor;
            AngleInDegree = initialRecValues.AngleInDegree;
        }

        public Vector2D[] GetCornerPoints()
        {
            var localPoints = new Vector2D[]
            {
                new Vector2D(0,0),
                new Vector2D(OriginalSize.Width, 0),
                new Vector2D(OriginalSize.Width, OriginalSize.Height),
                new Vector2D(0, OriginalSize.Height),
            };

            return localPoints
                .Select(x => x - new Vector2D(LocalPivot.X * OriginalSize.Width, LocalPivot.Y * OriginalSize.Height)) //Schritt 1: Verschiebe den lokalen Pivotpunkt zum Nullpunkt
                .Select(x => x * SizeFactor) //Schritt 2: Skaliere die Größe
                .Select(x => Vector2D.RotatePointAroundPivotPoint(new Vector2D(0, 0), x, AngleInDegree)) //Schritt 3: Drehe die Punkte
                .Select(x => x + PivotPoint) //Schritt 4: Gehe zum globalen Pivotpoint
                .ToArray();
        }

        public bool IsPointInside(Vector2D point)
        {
            return MathHelper.PointIsInsidePolygon(GetCornerPoints(), point);
        }

        public RectangleF GetBoundingBox()
        {
            return MathHelper.GetBoundingBoxFromPolygon(GetCornerPoints());
        }

        public void DefinePivotPoint(Vector2D point)
        {
            var points = GetCornerPoints();
            Vector2D dirX = (points[1] - points[0]).Normalize();
            Vector2D dirY = (points[3] - points[0]).Normalize();

            float localX = (dirX * (point - points[0])) / (OriginalSize.Width * SizeFactor);
            float localY = (dirY * (point - points[0])) / (OriginalSize.Height * SizeFactor);

            var oldLocal = this.LocalPivot;
            this.LocalPivot = new Vector2D(localX, localY);
            var diff = this.LocalPivot - oldLocal;
            this.PivotPoint += (dirX * diff.X * OriginalSize.Width + dirY * diff.Y * OriginalSize.Height) * SizeFactor;
        }

        public Matrix4x4 GetLocalToScreenMatrix()
        {
            var m = Matrix4x4.Ident();

            m *= Matrix4x4.Translate(-LocalPivot.X * OriginalSize.Width, -LocalPivot.Y * OriginalSize.Height, 0); //Schritt 1: Verschiebe den PivotPunkt zum Nullpunkt
            m *= Matrix4x4.Scale(SizeFactor, SizeFactor, SizeFactor);  //Schritt 2: Skaliere
            m *= Matrix4x4.Rotate(AngleInDegree, 0, 0, 1);             //Schritt 3: Rotiere
            m *= Matrix4x4.Translate(+PivotPoint.X, +PivotPoint.Y, 0); //Schritt 4: Zurück zum Pivotpunkt


            return m;
        }
    }
}
