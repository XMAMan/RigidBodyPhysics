using DynamicObjCreation.RigidBodyDestroying.FortuneVoronio;
using PhysicGlobal;
using System.Drawing;

namespace DynamicObjCreation.RigidBodyDestroying
{
    public static class VoronoiHelper
    {
        public static List<Vec2D[]> CreateVoronoi(float width, float height, int cellPointCount, Random rand)
        {
            List<Vec2D[]> polys = new List<Vec2D[]>();

            var texSize = new Size((int)width, (int)height);
            var voronoiCellPoints = GetRandomPointList(cellPointCount, texSize.Width, texSize.Height, rand);
            var voronioPolygons = Voronoi.GetVoronoiPolygons(texSize, voronoiCellPoints);

            //Sorge dafür, dass alle Voronoi-Polygone CCW sind
            for (int i = 0; i < voronioPolygons.Count; i++)
            {
                var voroPoly = voronioPolygons[i];
                if (PolygonHelper.IsPolygonCounterClockWise(voroPoly.Select(x => new Vec2D(x.X, x.Y)).ToArray()) == false)
                {
                    var list = voroPoly.ToList();
                    list.Reverse();
                    voronioPolygons[i] = list.ToArray();
                }

                polys.Add(voronioPolygons[i]);
            }

            return polys;
        }

        private static List<Point> GetRandomPointList(int cellPointCount, int maxX, int maxY, Random rand)
        {
            List<Point> cellPoints = new List<Point>();
            for (int i = 0; i < cellPointCount; i++)
            {
                Point P = new Point((int)(rand.NextDouble() * maxX), (int)(rand.NextDouble() * maxY));
                if (!cellPoints.Contains(P)) cellPoints.Add(P);
            }
            return cellPoints;
        }
    }
}
