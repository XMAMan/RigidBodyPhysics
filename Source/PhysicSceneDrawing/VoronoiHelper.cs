using GraphicPanels;
using PhysicGlobal;

namespace PhysicSceneDrawing
{
    public static class VoronoiHelper
    {
        public static List<Vec2D[]> CreateVoronoi(float width, float height, int cellPointCount, Random rand)
        {
            List<Vec2D[]> polys = new List<Vec2D[]>();

            var texSize = new Size((int)width, (int)height);
            var voronoiCellPoints = GraphicPanel2D.GetRandomPointList(cellPointCount, texSize.Width, texSize.Height, rand);
            var voronioPolygons = GraphicPanel2D.GetVoronoiPolygons(texSize, voronoiCellPoints);

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

                polys.Add(voronioPolygons[i].Select(x => new Vec2D(x.Position.X, x.Position.Y)).ToArray());
            }

            return polys;
        }
    }
}
