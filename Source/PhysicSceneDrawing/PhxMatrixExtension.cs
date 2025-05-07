using PhysicGlobal;

namespace PhysicSceneDrawing
{
    public static class PhxMatrixExtension
    {
        public static GraphicMinimal.Matrix4x4 To4x4Matrix(this PhxMatrix matrix)
        {
            return new GraphicMinimal.Matrix4x4(matrix.Values);
        }
    }
}
