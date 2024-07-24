using RigidBodyPhysics.RuntimeObjects.RigidBody;
using TextureEditorGlobal;

namespace DynamicObjCreation.RigidBodyDestroying
{
    internal interface IRectangleDestroyer
    {
        BodyWithTexture[] Destroy(IPublicRigidBody body, TextureExportData textureExport);
    }

    //Möglichkeit 1: Es wird ein Rechteck erzeugt, was genau beim TexturRechteck liegt
    internal class ReplaceTexRecWithPhysikRec : IRectangleDestroyer
    {
        private DestroyWithSingleBoxParameter parameter;
        public ReplaceTexRecWithPhysikRec(DestroyWithSingleBoxParameter parameter)
        {
            this.parameter = parameter;
        }

        public BodyWithTexture[] Destroy(IPublicRigidBody body, TextureExportData textureExport)
        {
            return DestroyHelper.DestroyTextureRectangle(body, new DestroyHelper.HelperParameter()
            {
                TextureExport = textureExport,
                LocalPolyCreator = (width, height) => PolygonInBoxCreator.CreateSingleBox(textureExport.Width, textureExport.Height),
                UserParameter = this.parameter
            });
        }
    }

    //Möglichkeit 2: Das TexturRechteck wird in lauter axiale Unterrechtecke zerlegt
    internal class ReplaceTexRecWithBoxes : IRectangleDestroyer
    {
        private DestroyWithBoxesParameter parameter;
        public ReplaceTexRecWithBoxes(DestroyWithBoxesParameter parameter)
        {
            this.parameter = parameter;
        }
        public BodyWithTexture[] Destroy(IPublicRigidBody body, TextureExportData textureExport)
        {
            int count = this.parameter.BoxCount;
            return DestroyHelper.DestroyTextureRectangle(body, new DestroyHelper.HelperParameter()
            {
                TextureExport = textureExport,
                LocalPolyCreator = (width, height) => PolygonInBoxCreator.CreateSmallBoxes(textureExport.Width, textureExport.Height, count),
                UserParameter = this.parameter
            });
        }
    }

    //Möglichkeit 3: Das TexturRechteck wird per Voronoi zerlegt
    internal class ReplaceTexRecWithVoronoi : IRectangleDestroyer
    {
        private DestroyWithVoronoiParameter parameter;
        public ReplaceTexRecWithVoronoi(DestroyWithVoronoiParameter parameter)
        {
            this.parameter = parameter;
        }

        public BodyWithTexture[] Destroy(IPublicRigidBody body, TextureExportData textureExport)
        {
            int cellPointCount = parameter.CellCount;
            Random rand = parameter.Rand;
            return DestroyHelper.DestroyTextureRectangle(body, new DestroyHelper.HelperParameter()
            {
                TextureExport = textureExport,
                LocalPolyCreator = (width, height) => PolygonInBoxCreator.CreateVoronoi(textureExport.Width, textureExport.Height, cellPointCount, rand),
                UserParameter = this.parameter
            });
        }
    }
}
