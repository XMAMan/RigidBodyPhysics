using GraphicPanels;
using RigidBodyPhysics.MathHelper;
using System.Drawing;
using PolyHelp = RigidBodyPhysics.MathHelper.PolygonHelper;

namespace DynamicObjCreation.RigidBodyDestroying
{
    //Erstellt innerhalb eines axialen Rechtecks eine Menge von Polygonen, welche das Rechteck ausfüllen
    internal static class PolygonInBoxCreator
    {
        public static List<Vec2D[]> CreateSingleBox(float width, float height)
        {
            return new List<Vec2D[]>()
            {
                new Vec2D[]
                {
                    new Vec2D(0,0),
                    new Vec2D(width, 0),
                    new Vec2D(width, height),
                    new Vec2D(0,height),
                }
            };
        }

        public static List<Vec2D[]> CreateSmallBoxes(float width, float height, int count)
        {
            Vec2D smallSize = new Vec2D(width / count, height / count); //Größe vom kleinen Kästchen

            List<Vec2D[]> polys = new List<Vec2D[]>();

            for (int x = 0; x < count; x++)
                for (int y = 0; y < count; y++)
                {
                    var topLeft = new Vec2D(x * smallSize.X, y * smallSize.Y);
                    polys.Add(new Vec2D[]
                    {
                        topLeft,
                        new Vec2D(topLeft.X + smallSize.X, topLeft.Y),
                        new Vec2D(topLeft.X + smallSize.X, topLeft.Y +  smallSize.Y),
                        new Vec2D(topLeft.X, topLeft.Y +  smallSize.Y),
                    });
                }

            return polys;
        }

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
                if (PolyHelp.IsCCW(voroPoly.Select(x => new Vec2D(x.X, x.Y)).ToArray()) == false)
                {
                    var list = voroPoly.ToList();
                    list.Reverse();
                    voronioPolygons[i] = list.ToArray();
                }

                polys.Add(voronioPolygons[i].Select(x => x.Position.ToPhx()).ToArray());
            }

            return polys;
        }
    }
}
