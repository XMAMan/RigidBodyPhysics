using PhysicGlobal;

namespace WpfControls.Controls.CameraSetting
{
    public static class Camera2DExtension
    {
        //Diese Funktion macht das gleiche wie PointToScreen nur dass ich eine Matrix dafür nutzen kann:
        //Vector2D point = Matrix4x4.MultPosition(camera.GetPointToSceenMatrix(), new Vector3D(point.X, point.Y, 0)).XY
        public static GraphicMinimal.Matrix4x4 GetPointToSceenMatrix(this Camera2D camera)
        {
            if (camera.ShowOriginalPosition)
                return GraphicMinimal.Matrix4x4.Ident();

            return GraphicMinimal.Matrix4x4.Translate(-camera.X, -camera.Y, 0) * GraphicMinimal.Matrix4x4.Scale(camera.ScaleFactor, camera.ScaleFactor, 1);
        }

        public static GraphicMinimal.Matrix4x4 GetPointToCameraMatrix(this Camera2D camera)
        {
            if (camera.ShowOriginalPosition)
                return GraphicMinimal.Matrix4x4.Ident();

            return GraphicMinimal.Matrix4x4.Scale(1.0f / camera.ScaleFactor, 1.0f / camera.ScaleFactor, 1) * GraphicMinimal.Matrix4x4.Translate(camera.X, camera.Y, 0);
        }
    }
}
