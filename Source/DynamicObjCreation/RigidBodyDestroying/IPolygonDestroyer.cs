using RigidBodyPhysics.RuntimeObjects.RigidBody;
using TextureEditorGlobal;

namespace DynamicObjCreation.RigidBodyDestroying
{
    internal interface IPolygonDestroyer
    {
        BodyWithTexture[] Destroy(IPublicRigidPolygon body, TextureExportData textureExport);
    }

    //Möglichkeit 1: Es wird ein Polygon erzeugt, was genau an der Stelle vom PhysikPolygon liegt
    internal class ReplacePolygonWithPolygon : IPolygonDestroyer
    {
        private DestroyWithSingleBoxParameter parameter;
        public ReplacePolygonWithPolygon(DestroyWithSingleBoxParameter parameter)
        {
            this.parameter = parameter;
        }

        public BodyWithTexture[] Destroy(IPublicRigidPolygon body, TextureExportData textureExport)
        {
            return DestroyHelper.DestroyPolygon(body, new DestroyHelper.HelperParameter()
            {
                TextureExport = textureExport,
                LocalPolyCreator = PolygonInBoxCreator.CreateSingleBox,
                UserParameter = this.parameter,
            });
        }
    }

    //Möglichkeit 2: Das Polygon wird in lauter axiale Unterrechtecke zerlegt
    internal class ReplacePolygonWithBoxes : IPolygonDestroyer
    {
        private DestroyWithBoxesParameter parameter;
        public ReplacePolygonWithBoxes(DestroyWithBoxesParameter parameter)
        {
            this.parameter = parameter;
        }

        public BodyWithTexture[] Destroy(IPublicRigidPolygon body, TextureExportData textureExport)
        {
            int count = this.parameter.BoxCount;
            return DestroyHelper.DestroyPolygon(body, new DestroyHelper.HelperParameter()
            {
                TextureExport = textureExport,
                LocalPolyCreator = (width, height) => PolygonInBoxCreator.CreateSmallBoxes(width, height, count),
                UserParameter = this.parameter,
            });
        }
    }

    //Möglichkeit 3: Das Polygon wird per Voronoi zerlegt
    internal class ReplacePolygonWithVoronoi : IPolygonDestroyer
    {
        private DestroyWithVoronoiParameter parameter;
        public ReplacePolygonWithVoronoi(DestroyWithVoronoiParameter parameter)
        {
            this.parameter = parameter;
        }

        public BodyWithTexture[] Destroy(IPublicRigidPolygon body, TextureExportData textureExport)
        {
            int cellPointCount = parameter.CellCount;
            Random rand = parameter.Rand;
            return DestroyHelper.DestroyPolygon(body, new DestroyHelper.HelperParameter() 
            { 
                TextureExport = textureExport, 
                LocalPolyCreator = (width, height) => PolygonInBoxCreator.CreateVoronoi(width, height, cellPointCount, rand),
                UserParameter = this.parameter,
            });
        }
    }
}
