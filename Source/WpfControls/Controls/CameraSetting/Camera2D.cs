using System;
using System.Drawing;

namespace WpfControls.Controls.CameraSetting
{
    //Stellt ein axial ausgerichtetes Rechteck im 4. Quadrant dar. Das ist das Sichtfenster für Objetke im 4. Quadrant
    //Die Zeichenfläche liegt im Bereich von X=0..ScreenWdith und Y=0..ScreenHeight -> Das ist der ScreenSpace
    //Das Kamerasichtfenster geht von X=min.X..min.X+screenWidth/factor und Y=min.Y..min.Y+screenHeight/factor -> Das ist der CameraSpace
    public class Camera2D
    {
        private PointF min = new PointF(0, 0); //Camera-MinPoint
        private float screenWidth;
        private float screenHeight;
        private float factor = 1;   //Um diesen Faktor muss die Scene skaliert werden, damit sie genau ins Fenster passt (Sie stößt dann oben/unten oder links/rechts an)
        private RectangleF? box = null; //Das ist die BoundingBox von den Objekt, was die Kamera anzeigen soll
        private float zoom = 1;
        private Size backgroundImage = new Size(100,100); //Größe vom Hintergrundbild

        //An diese Position wird die linke obere Ecke von der Kamera (min-Variable) platziert, wenn der AutoZoom aktiviert wird
        public enum InitialPositionIfAutoZoomIsActivated
        {
            SceneCenterToScreenCenter,  //Wenn der AutoZoom aktiviert wird, wird die Mitte des SceneBoundingbox bei der Bildschirmmitte angezeigt
            ToLeftTopCorner, //Die linke obere Ecke von der SceneBoundingbox wird an der linken oberen Bildschirmecke angezeigt
            ToBackgroundImage, //Die Kamera zeigt den Bereich (0,0) - (backgroundImage.Width, backgroundImage.Height)
        }

        public InitialPositionIfAutoZoomIsActivated InitialPosition = InitialPositionIfAutoZoomIsActivated.SceneCenterToScreenCenter;

        //Position vom Kamera-Spacepunkt
        public float X
        {
            get { return min.X; }
            set { min.X = value; }
        }
        public float Y
        {
            get { return min.Y; }
            set { min.Y = value; }
        }

        //Sichtfenster im Camera-Space
        public RectangleF GetScreenBox()
        {
            if (this.ShowOriginalPosition) return new RectangleF(0, 0, screenWidth, screenHeight);

            return new RectangleF(X, Y, screenWidth / factor, screenHeight / factor);
        }

        //Größe der Szene im Cameraspace
        public RectangleF? GetSceneBoundingBox()
        {
            return box;
        }

        public float Zoom
        {
            get { return this.zoom; }
            set { this.zoom = value; UpdateScaleFactor(); }
        }

        public bool ShowOriginalPosition { get; set; } = false;

        //Der Cameraspace wird so angeordnet, dass er die Box zeigt aber er hält die Seitenverhältnisse vom Screen bei
        public Camera2D(int screenWidth, int screenHeight, RectangleF? boundingBoxFromScene = null)
        {
            box = boundingBoxFromScene;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;

            UpdateScaleFactor();
            SetInitialCameraPosition();
        }

        public void UpdateScreenSize(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            UpdateScaleFactor();
        }

        //Wird gerufen, wenn im Editor AutoZoom auf true gestellt wird. Damit passt die Scene Dank der Aktualisierung der
        //factor-Variable genau ins Bild. 
        public void UpdateSceneBoundingBox(RectangleF boundingBox)
        {
            this.box = boundingBox;
            UpdateScaleFactor();
        }

        public void UpdateBackgroundImage(Size size)
        {
            this.backgroundImage = size;
        }

        //Wird gerufen, wenn im Editor ein LevelItem hinzu gefügt oder gelöscht wurde.
        //So wird im SmallWindow immer die Gesamtszene gezeigt aber im Hauptfenster ändert sich nicht der Zoom/die Kameraposition.
        public void UpdateBoundingBoxWithoutZoomChange(RectangleF boundingBox)
        {
            this.box = boundingBox;
        }

        private void UpdateScaleFactor()
        {
            if (box != null)
            {
                factor = GetScaleFactor(new SizeF(screenWidth, screenHeight), box.Value.Size);
            }


            factor *= this.zoom;
        }

        //Gibt den Faktor zurück, um den die innerBox skaliert werden muss, damit dessen width oder height der outerBox entspricht
        public static float GetScaleFactor(SizeF outerBox, SizeF innerBox)
        {
            float factorX = outerBox.Width / innerBox.Width;
            float factorY = outerBox.Height / innerBox.Height;
            return Math.Min(factorX, factorY);
        }

        public static float GetScaleFactor(Size outerBox, Size innerBox)
        {
            float factorX = outerBox.Width / (float)innerBox.Width;
            float factorY = outerBox.Height / (float)innerBox.Height;
            return Math.Min(factorX, factorY);
        }

        public void SetInitialCameraPosition()
        {
            this.zoom = 1;

            UpdateScaleFactor();

            switch (this.InitialPosition)
            {
                case InitialPositionIfAutoZoomIsActivated.SceneCenterToScreenCenter:
                    {
                        PointF centerFromScene = new PointF(0, 0);
                        if (this.box != null)
                        {
                            centerFromScene = new PointF(box.Value.X + box.Value.Width / 2, box.Value.Y + box.Value.Height / 2);
                        }

                        PointF cameraCenter = new PointF((this.screenWidth / this.factor) / 2, (this.screenHeight / this.factor) / 2);
                        this.min = new PointF(centerFromScene.X - cameraCenter.X, centerFromScene.Y - cameraCenter.Y);
                    }
                    break;

                case InitialPositionIfAutoZoomIsActivated.ToLeftTopCorner:
                    this.min = new PointF(box.Value.X, box.Value.Y);
                    break;

                case InitialPositionIfAutoZoomIsActivated.ToBackgroundImage:
                    UpdateSceneBoundingBox(new RectangleF(0, 0, this.backgroundImage.Width, this.backgroundImage.Height));
                    UpdateScaleFactor();
                    this.min = new PointF(0, 0);                    
                    break;
            }
        }

        //ScreenSpace to Cameraspace
        public PointF PointToCamera(PointF point)
        {
            if (this.ShowOriginalPosition) return point;

            return new PointF(min.X + point.X / this.factor, min.Y + point.Y / this.factor);
        }

        public float LengthToCamera(float length)
        {
            if (this.ShowOriginalPosition) return length;

            return length / this.factor;
        }

        //Cameraspace to ScreenSpace
        public PointF PointToScreen(PointF point)
        {
            if (this.ShowOriginalPosition) return point;

            return new PointF((point.X - min.X) * this.factor, (point.Y - min.Y) * this.factor);
        }

        public float LengthToScreen(float length)
        {
            if (this.ShowOriginalPosition) return length;

            return length * this.factor;
        }

        //Diese Funktion macht das gleiche wie PointToScreen nur dass ich eine Matrix dafür nutzen kann:
        //Vector2D point = Matrix4x4.MultPosition(camera.GetPointToSceenMatrix(), new Vector3D(point.X, point.Y, 0)).XY
        public GraphicMinimal.Matrix4x4 GetPointToSceenMatrix()
        {
            if (this.ShowOriginalPosition)
                return GraphicMinimal.Matrix4x4.Ident();

            return GraphicMinimal.Matrix4x4.Translate(-this.X, -this.Y, 0) * GraphicMinimal.Matrix4x4.Scale(this.factor, this.factor, 1);
        }

        public GraphicMinimal.Matrix4x4 GetPointToCameraMatrix()
        {
            if (this.ShowOriginalPosition)
                return GraphicMinimal.Matrix4x4.Ident();

            return GraphicMinimal.Matrix4x4.Scale(1.0f / this.factor, 1.0f / this.factor, 1) * GraphicMinimal.Matrix4x4.Translate(this.X, this.Y, 0);
        }
    }
}
