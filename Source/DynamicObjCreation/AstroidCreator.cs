using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System.Drawing;
using TextureEditorGlobal;

namespace DynamicObjCreation
{

    //Erzeugt ein Zufallspolygon, was wie ein Stein aussieht
    public static class AstroidCreator
    {
        public static BodyWithTexture CreateAstroid(Vec2D position, Random rand, float size, int pointCount, string textureFile, Vec2D velocity, float angularVelocity)
        {
            var poly = CreateAstroidPolygon(rand, size, pointCount);
            var center = PolygonHelper.GetCenterOfMassFromPolygon(poly);

            var polyBody = new PolygonExportData()
            {
                PolygonType = PolygonCollisionType.Rigid,
                Points = poly.Select(x => x - center).ToArray(),
                Center = position,
                AngleInDegree = 0,
                Velocity = velocity,
                AngularVelocity = angularVelocity,
                MassData = new MassData(MassData.MassType.Density, 1, 0.001f),
                Friction = 1.5f,
                Restituion = 0.5f,
                CollisionCategory = 0
            };

            var polyBox = PolygonHelper.GetBoundingBoxFromPolygon(poly);

            var texture = new TextureExportData()
            {
                TextureFile = textureFile,
                MakeFirstPixelTransparent = false,
                ColorFactor = Color.White,
                DeltaX = 0,
                DeltaY = 0,
                Width = (int)polyBox.Width,
                Height = (int)polyBox.Height,
                DeltaAngle = 0,
                ZValue = 0
            };

            return new BodyWithTexture(polyBody, texture, 0, new string[0]);
        }

        public static Vec2D[] CreateAstroidPolygon(Random rand, float size, int pointCount)
        {
            List<Vec2D> points = new List<Vec2D>();

            float radius = (float)(rand.NextDouble() * 0.3 + 0.7) * size; //Ändert sich mit den i-Schritt
            float xRadius = (float)(rand.NextDouble() * 0.5 + 0.5);
            float yRadius = (float)(rand.NextDouble() * 0.5 + 0.5);
            float phiAdd = (float)rand.NextDouble() * 360;

            for (int i=0;i<pointCount; i++)
            {
                double phi = i / (double)pointCount * 2 * Math.PI;
                var point = new Vec2D((float)Math.Cos(phi) * radius * xRadius, (float)Math.Sin(phi) * radius * yRadius);
                point = Vec2D.RotatePointAroundPivotPoint(new Vec2D(0, 0), point, phiAdd);
                points.Add(point);

                radius += (float)(rand.NextDouble()-0.5) * (size / 3);
                if (radius < 0) radius = -radius;
                if (radius > size)
                {
                    float diff = Math.Min(size, radius - size);
                    radius = size - diff;
                }
            }

             

            return points.ToArray();
        }
    }
}
