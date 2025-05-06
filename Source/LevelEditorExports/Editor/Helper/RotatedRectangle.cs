using LevelEditorExports.Editor.Prototyps;
using PhysicGlobal;
using System.Drawing;

namespace LevelEditorExports.Editor.Helper
{
    //Das ist ein Rechteck, was im Original-Zustand OriginalSize groß ist und was dann aber noch um SizeFactor skaliert wurde
    //und um AngleInDegree gedreht wurde. Der Drehpunkt ist der PivotPoint. Dieser Punkt liegt aber nicht zwangsweise im Rechteck-Zentrum.
    public class RotatedRectangle
    {
        public Vec2D PivotPoint { get; set; } //Das ist die Mausklickposition aus der AddItem/MoveSelect-Funktion
        public SizeF OriginalSize { get; set; } //Breite und höhe des Objektes wenn es nicht gedreht/skaliert wurde
        public Vec2D LocalPivot { get; set; } //Vektor der von der linken oberen Ecke zum Pivot-Punkt zeigt (0,0) = Linke obere Ecke; (1,0) = Rechte obere Ecke; (1,1) = Reche untere Ecke
        public float SizeFactor { get; set; }
        public float AngleInDegree { get; set; }

        public RotatedRectangle(Vec2D pivotPoint, SizeF originalSize, InitialRotatedRectangleValues initialRecValues)
        {
            PivotPoint = pivotPoint;
            OriginalSize = originalSize;
            LocalPivot = initialRecValues.LocalPivot;
            SizeFactor = initialRecValues.SizeFactor;
            AngleInDegree = initialRecValues.AngleInDegree;
        }

        public Vec2D[] GetCornerPoints()
        {
            var localPoints = new Vec2D[]
            {
                new Vec2D(0,0),
                new Vec2D(OriginalSize.Width, 0),
                new Vec2D(OriginalSize.Width, OriginalSize.Height),
                new Vec2D(0, OriginalSize.Height),
            };

            return localPoints
                .Select(x => x - new Vec2D(LocalPivot.X * OriginalSize.Width, LocalPivot.Y * OriginalSize.Height)) //Schritt 1: Verschiebe den lokalen Pivotpunkt zum Nullpunkt
                .Select(x => x * SizeFactor) //Schritt 2: Skaliere die Größe
                .Select(x => Vec2D.RotatePointAroundPivotPoint(new Vec2D(0, 0), x, AngleInDegree)) //Schritt 3: Drehe die Punkte
                .Select(x => x + PivotPoint) //Schritt 4: Gehe zum globalen Pivotpoint
                .ToArray();
        }

        public bool IsPointInside(Vec2D point)
        {
            return PhysicGlobal.PolygonHelper.PointIsInsidePolygon(GetCornerPoints(), point);
        }

        public PhysicGlobal.BoundingBox GetBoundingBox()
        {
            return PolygonHelper.GetBoundingBoxFromPolygon(GetCornerPoints());
        }

        public void DefinePivotPoint(Vec2D point)
        {
            var points = GetCornerPoints();
            Vec2D dirX = (points[1] - points[0]).Normalize();
            Vec2D dirY = (points[3] - points[0]).Normalize();

            float localX = (dirX * (point - points[0])) / (OriginalSize.Width * SizeFactor);
            float localY = (dirY * (point - points[0])) / (OriginalSize.Height * SizeFactor);

            var oldLocal = this.LocalPivot;
            this.LocalPivot = new Vec2D(localX, localY);
            var diff = this.LocalPivot - oldLocal;
            this.PivotPoint += (dirX * diff.X * OriginalSize.Width + dirY * diff.Y * OriginalSize.Height) * SizeFactor;
        }

        public PhxMatrix GetLocalToScreenMatrix()
        {
            var m = PhxMatrix.Ident();

            m *= PhxMatrix.Translate(-LocalPivot.X * OriginalSize.Width, -LocalPivot.Y * OriginalSize.Height, 0); //Schritt 1: Verschiebe den PivotPunkt zum Nullpunkt
            m *= PhxMatrix.Scale(SizeFactor, SizeFactor, SizeFactor);  //Schritt 2: Skaliere
            m *= PhxMatrix.Rotate(AngleInDegree, 0, 0, 1);             //Schritt 3: Rotiere
            m *= PhxMatrix.Translate(+PivotPoint.X, +PivotPoint.Y, 0); //Schritt 4: Zurück zum Pivotpunkt


            return m;
        }
    }
}
