namespace DrawingPanel
{
    internal static class PhxMatrixExtension
    {
        public static GraphicMinimal.Matrix4x4 To4x4Matrix(this PhysicGlobal.Matrix4x4 matrix)
        {
            return new GraphicMinimal.Matrix4x4(matrix.Values);
        }

        public static PhysicGlobal.Matrix4x4 ToPhxMatrix(this GraphicMinimal.Matrix4x4 matrix)
        {
            return new PhysicGlobal.Matrix4x4(matrix.Values);
        }
    }
}
