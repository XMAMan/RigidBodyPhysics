using PhysicGlobal;
using PhysicSceneDrawing;

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
            return VoronoiHelper.CreateVoronoi(width, height, cellPointCount, rand);
        }
    }
}
