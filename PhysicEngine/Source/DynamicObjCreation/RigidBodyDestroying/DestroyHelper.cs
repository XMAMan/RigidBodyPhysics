using DynamicObjCreation.PolygonIntersection;
using GraphicMinimal;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using TextureEditorGlobal;

namespace DynamicObjCreation.RigidBodyDestroying
{
    //Wird bei bei der Zerlegung von IPublicRigidBody und IPublicRigidPolygon-Objekten in ein BodyWithTexture-Array verwendet
    internal static class DestroyHelper
    {
        public class HelperParameter
        {
            public TextureExportData TextureExport;

            public delegate List<Vec2D[]> LocalPolyCreatorFunction(float width, float height);
            public LocalPolyCreatorFunction LocalPolyCreator;
            public IRigidDestroyerParameter UserParameter;
        }

        //public static BodyWithTexture[] DestroyTextureRectangle(IPublicRigidBody body, TextureExportData textureExport, Func<float, float, List<Vec2D[]>> localPolyCreator)
        public static BodyWithTexture[] DestroyTextureRectangle(IPublicRigidBody body, HelperParameter p)
        {
            List<BodyWithTexture> returnList = new List<BodyWithTexture>();

            float bodyAngleInDegree = body.Angle / (float)Math.PI * 180;
            var texCenter = Vec2D.RotatePointAroundPivotPoint(body.Center, body.Center + new Vec2D(p.TextureExport.DeltaX, p.TextureExport.DeltaY), bodyAngleInDegree);

            float angleInDegree = bodyAngleInDegree + p.TextureExport.DeltaAngle;

            var localPolys = p.LocalPolyCreator(p.TextureExport.Width, p.TextureExport.Height);

            var m = Matrix3x3.Ident();
            m *= Matrix3x3.Translate(-p.TextureExport.Width / 2, -p.TextureExport.Height / 2);//Schritt 1: Verschiebe den PivotPunkt zum Nullpunkt
            m *= Matrix3x3.Rotate(angleInDegree);                                             //Schritt 2: Rotiere um Z
            m *= Matrix3x3.Translate(texCenter.X, texCenter.Y);                               //Schritt 3: Gehe zum Zielpunkt

            foreach (var localPoly in localPolys)
            {
                var poly = localPoly.Select(x => Matrix3x3.MultPosition(m, x.ToGrx()).ToPhx()).ToArray();
                returnList.Add(CreateTexturedPolygon(poly, texCenter, body, p));
            }

            return returnList.ToArray();
        }

        public static BodyWithTexture[] DestroyPolygon(IPublicRigidPolygon body, HelperParameter p)
        {
            List<BodyWithTexture> returnList = new List<BodyWithTexture>();

            var texCenter = GetTextureCenterFromPolygon(body, p.TextureExport);

            float angleInDegree = body.Angle / (float)Math.PI * 180 + p.TextureExport.DeltaAngle;

            var rotatedBox = new RotatedBoundingBox(body.Vertex, angleInDegree);
            var localPolys = p.LocalPolyCreator(rotatedBox.Width + 2, rotatedBox.Height + 2); //+2 steht hier, da es sonst bei der Line-Line-Intersection beim PolygonIntersector zu Rundungsfehlern kommt 

            var m = Matrix3x3.Ident();
            m *= Matrix3x3.Translate(-rotatedBox.Width / 2 - 1, -rotatedBox.Height / 2 - 1); //Schritt 1: Verschiebe den PivotPunkt zum Nullpunkt (Auch hier steht -1 wegen den Rundungsfehler bei PolyIntersections.GetIntersectionPointBetweenToLines) (Siehe Intersect11-Testcase)
            m *= Matrix3x3.Rotate(angleInDegree);                                            //Schritt 2: Rotiere um Z
            m *= Matrix3x3.Translate(rotatedBox.Center.X, rotatedBox.Center.Y);              //Schritt 3: Gehe zum Zielpunkt

            foreach (var localPoly in localPolys)
            {
                var polyPoints = localPoly.Select(x => Matrix3x3.MultPosition(m, x.ToGrx()).ToPhx()).ToArray();

                var polygons = PolygonIntersector.GetIntersection(body.Vertex, polyPoints);
                if (polygons == null) continue;

                foreach (var poly in polygons)
                {
                    returnList.Add(CreateTexturedPolygon(poly, texCenter, body, p));
                }

            }

            return returnList.ToArray();
        }

        private static Vec2D GetTextureCenterFromPolygon(IPublicRigidPolygon body, TextureExportData p)
        {
            float angleInDegree = body.Angle / (float)Math.PI * 180;
            var localCenter = RigidBodyPhysics.MathHelper.BoundingBox.GetBoxFromPoints(((PolygonExportData)body.GetExportData()).Points).Center.ToGrx();
            return TextureRectangleHelper.GetTextureBorderPoints(body.Center.ToGrx(), localCenter, p.Width, p.Height, angleInDegree, p.DeltaX, p.DeltaY, p.DeltaAngle).Last().ToPhx();
        }

        private static BodyWithTexture CreateTexturedPolygon(Vec2D[] poly, Vec2D texCenter, IPublicRigidBody body, HelperParameter p)
        {
            var bodyExport = body.GetExportData();
            float angleInDegree = bodyExport.AngleInDegree + p.TextureExport.DeltaAngle;
            var center = PolygonHelper.GetCenterOfMassFromPolygon(poly);

            var polyBody = new PolygonExportData()
            {
                PolygonType = PolygonCollisionType.Rigid,
                Points = poly.Select(x => x - center).ToArray(),
                Center = center,
                AngleInDegree = 0,
                Velocity = body.Velocity,
                AngularVelocity = body.AngularVelocity,
                MassData = bodyExport.MassData,
                Friction = bodyExport.Friction,
                Restituion = bodyExport.Restituion,
                CollisionCategory = bodyExport.CollisionCategory
            };

            polyBody = p.UserParameter.TransformFunc(body, polyBody);

            var toPolyCenter = RigidBodyPhysics.MathHelper.BoundingBox.GetBoxFromPoints(poly).Center - texCenter;

            var texData = new TextureExportData()
            {
                TextureFile = p.TextureExport.TextureFile,
                MakeFirstPixelTransparent = p.TextureExport.MakeFirstPixelTransparent,
                ColorFactor = p.TextureExport.ColorFactor,
                DeltaX = -toPolyCenter.X,
                DeltaY = -toPolyCenter.Y,
                Width = p.TextureExport.Width,
                Height = p.TextureExport.Height,
                DeltaAngle = angleInDegree,
                ZValue = p.TextureExport.ZValue,
                IsInvisible = false
            };

            var bodyWithTex = new BodyWithTexture(polyBody, texData, 0, new string[0]);

            return bodyWithTex;
        }
    }
}
