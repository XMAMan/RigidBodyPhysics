using RigidBodyPhysics.RuntimeObjects.RigidBody;
using TextureEditorGlobal;
using static DynamicObjCreation.RigidBodyDestroying.IRigidDestroyerParameter;

namespace DynamicObjCreation.RigidBodyDestroying
{
    //Hinweis: Beim Rechteck und Kreis hängt die Textur am Masseschwerpunkt
    //         Beim Polygon hängt es in der Mitte von der Boundingbox, welche das ungedrehte Polygon umschließt
    public class RigidBodyDestroyer
    {
        public BodyWithTexture[] Destroy(IRigidDestroyerParameter parameter, IPublicRigidBody body, TextureExportData textureExport)
        {
            //Zerlege Rechteck/Kreis
            if (body is IPublicRigidRectangle || body is IPublicRigidCircle)
            {
                return GetRectangleDestroyer(parameter).Destroy(body, textureExport);
            }

            //Zerlege Polygon
            if (body is IPublicRigidPolygon)
            {
                return GetPolygonDestroyer(parameter).Destroy((IPublicRigidPolygon)body, textureExport);
            }

            throw new NotImplementedException();
        }

        private IRectangleDestroyer GetRectangleDestroyer(IRigidDestroyerParameter parameter)
        {
            switch (parameter.Method)
            {
                case DestroyMethod.SingleBox:
                    return new ReplaceTexRecWithPhysikRec((DestroyWithSingleBoxParameter)parameter);

                case DestroyMethod.Boxes:
                    return new ReplaceTexRecWithBoxes((DestroyWithBoxesParameter)parameter);

                case DestroyMethod.Voronoi:
                    return new ReplaceTexRecWithVoronoi((DestroyWithVoronoiParameter)parameter);
            }

            throw new NotImplementedException();
        }

        private IPolygonDestroyer GetPolygonDestroyer(IRigidDestroyerParameter parameter)
        {
            switch (parameter.Method)
            {
                case DestroyMethod.SingleBox:
                    return new ReplacePolygonWithPolygon((DestroyWithSingleBoxParameter)parameter);

                case DestroyMethod.Boxes:
                    return new ReplacePolygonWithBoxes((DestroyWithBoxesParameter)parameter);

                case DestroyMethod.Voronoi:
                    return new ReplacePolygonWithVoronoi((DestroyWithVoronoiParameter)parameter);
            }

            throw new NotImplementedException();
        }
    }
}