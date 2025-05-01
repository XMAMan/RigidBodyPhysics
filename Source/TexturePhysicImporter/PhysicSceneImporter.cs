using JsonHelper;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics.ExportData.RigidBody;
using PhysicGlobal;
using System.Drawing;
using TextureEditorGlobal;

namespace TexturePhysicImporter
{
    public class PhysicSceneImporter : IVisualisizerImporter
    {
        private PhysicSceneExportData physicScene;
        public PhysicSceneImporter(string physicSceneJson)
        {
            var rawPhysicData = Helper.FromCompactJson<PhysicSceneExportData>(physicSceneJson);
            this.physicScene = rawPhysicData;
        }
        public PhysicSceneImporter(PhysicSceneExportData physicSceneData)
        {
            this.physicScene = physicSceneData;
        }
        public VisualisizerInputData Import()
        {
            return new VisualisizerInputData()
            {
                Shapes = physicScene.Bodies.Select(Convert).ToArray()
            };
        }

        private static I2DAreaShape Convert(IExportRigidBody body)
        {
            if (body is RectangleExportData)
            {
                return new RigidRectangle((RectangleExportData)body);
            }

            if (body is PolygonExportData)
            {
                return new RigidPolygon((PolygonExportData)body);
            }

            if (body is CircleExportData)
            {
                return new RigidCircle((CircleExportData)body);
            }

            throw new NotImplementedException();
        }
    }

    internal class RigidRectangle : IRectangle
    {
        private RectangleExportData r;
        public Vec2D Center { get => r.Center; }
        public float AngleInDegree { get => r.AngleInDegree; }
        public RectangleF LocalBoundingBox { get => new RectangleF(- r.Size.X / 2, - r.Size.Y / 2, r.Size.X, r.Size.Y); }
        public float Width { get => r.Size.X; }
        public float Height { get => r.Size.Y; }

        public RigidRectangle(RectangleExportData rec)
        {
            this.r = rec;
        }
    }

    internal class RigidPolygon : IPolygon
    {
        private PolygonExportData r;
        public Vec2D Center { get => r.Center; }
        public float AngleInDegree { get => r.AngleInDegree; }
        public RectangleF LocalBoundingBox { get => PhysicGlobal.BoundingBox.GetBoxFromPoints(r.Points).ToRectangleF(); }
        public Vec2D[] Points { get => r.Points.Select(x => Vec2D.RotatePointAroundPivotPoint(r.Center, r.Center + x, r.AngleInDegree)).ToArray(); }

        public RigidPolygon(PolygonExportData rec)
        {
            this.r = rec;
        }
    }

    internal class RigidCircle : ICircle
    {
        private CircleExportData r;
        public Vec2D Center { get => r.Center; }
        public float AngleInDegree { get => r.AngleInDegree; }
        public RectangleF LocalBoundingBox { get => new RectangleF(-r.Radius, -r.Radius, r.Radius * 2, r.Radius * 2); }
        public float Radius { get => r.Radius; }

        public RigidCircle(CircleExportData rec)
        {
            this.r = rec;
        }
    }
}
