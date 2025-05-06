using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics.ExportData.Joints;
using PhysicGlobal;

namespace LevelToSimulatorConverter.Helper
{
    //Hilfsfunktionen für ein PhysicSceneExportData-Objekt
    public static class PhysicSceneExportDataHelper
    {
        public static PhysicSceneExportData CreateCopyFromScene(PhysicSceneExportData scene)
        {
            string json = JsonHelper.Helper.ToJson(scene);
            var copy = JsonHelper.Helper.CreateFromJson<PhysicSceneExportData>(json);
            return copy;
        }

        //Hier wird eine Matrix4x4 und keine Matrix3x3 verwendet, weil an anderer Stelle mit der Matrix panel.MultTransformationMatrix aufgerufen wird
        //Man kann auch nicht so einfach eine Matrix3x3 in eine Matrix4x4 (oder umgekehrt) umwandeln, da die Translate-Matrix
        //einmal das XY in Zeile 3 und einmal in Zeile 4 stehen hat.
        public static void TranslateScene(PhysicSceneExportData scene, PhxMatrix matrix)
        {
            float angleInDegreeMatrix = PhxMatrix.GetAngleInDegreeFromMatrix(matrix);
            float sizeFactorMatrix = PhxMatrix.GetSizeFactorFromMatrix(matrix);

            foreach (var body in scene.Bodies)
            {
                body.Center = PhxMatrix.MultPosition(matrix, new Vec2D(body.Center.X, body.Center.Y));

                body.AngleInDegree += angleInDegreeMatrix;

                if (body is RectangleExportData)
                {
                    var rect = (RectangleExportData)body;
                    rect.Size *= sizeFactorMatrix;
                }

                if (body is CircleExportData)
                {
                    var circle = (CircleExportData)body;
                    circle.Radius *= sizeFactorMatrix;
                }

                if (body is PolygonExportData)
                {
                    var polygon = (PolygonExportData)body;
                    foreach (var point in polygon.Points)
                    {
                        var translatedPoint = point * sizeFactorMatrix;
                        point.X = translatedPoint.X;
                        point.Y = translatedPoint.Y;
                    }
                }
            }

            foreach (var joint in scene.Joints)
            {
                joint.R1 *= sizeFactorMatrix;
                joint.R2 *= sizeFactorMatrix;

                if (joint is DistanceJointExportData)
                {
                    //Angabe der Min/Max-Länge erfolgt in Pixeln. Deswegen muss das mit skaliert werden
                    var disJoint = (DistanceJointExportData)joint;
                    disJoint.MinLength *= sizeFactorMatrix;
                    disJoint.MaxLength *= sizeFactorMatrix;
                }
            }

            foreach (var thruster in scene.Thrusters)
            {
                thruster.R1 *= sizeFactorMatrix;
            }

            foreach (var axialFriction in scene.AxialFrictions)
            {
                axialFriction.R1 *= sizeFactorMatrix;
            }
        }

        public static BoundingBox GetBoundingBoxFromScene(PhysicSceneExportData data)
        {
            return BoundingBox.GetBoxFromBoxes(data.Bodies.Select(GetBoundingBox));
        }

        private static BoundingBox GetBoundingBox(IExportRigidBody body)
        {
            if (body is RectangleExportData)
                return GetBoundingBox((RectangleExportData)body);

            if (body is CircleExportData)
                return GetBoundingBox((CircleExportData)body);

            if (body is PolygonExportData)
                return GetBoundingBox((PolygonExportData)body);

            throw new NotImplementedException();
        }

        private static BoundingBox GetBoundingBox(RectangleExportData r)
        {
            var size = r.Size;
            var vertexLocal = new Vec2D[]
            {
                new Vec2D(-size.X / 2, -size.Y / 2), //TopLeft
                new Vec2D(+size.X / 2, -size.Y / 2), //TopRight
                new Vec2D(+size.X / 2, +size.Y / 2), //BottomRight
                new Vec2D(-size.X / 2, +size.Y / 2), //BottomLeft
            };

            var cornerPoints = vertexLocal.Select(x => r.Center + Vec2D.RotatePointAroundPivotPoint(new Vec2D(0, 0), x, r.AngleInDegree)).ToArray();

            return BoundingBox.GetBoxFromPoints(cornerPoints);
        }

        private static BoundingBox GetBoundingBox(CircleExportData r)
        {
            return new BoundingBox(new Vec2D(r.Center.X - r.Radius, r.Center.Y - r.Radius),
                    new Vec2D(r.Center.X + r.Radius, r.Center.Y + r.Radius));
        }

        private static BoundingBox GetBoundingBox(PolygonExportData r)
        {
            var points = r.Points.Select(x => r.Center + x).ToArray();

            return new BoundingBox(new Vec2D(points.Min(x => x.X), points.Min(x => x.Y)),
                    new Vec2D(points.Max(x => x.X), points.Max(x => x.Y)));
        }
    }
}
