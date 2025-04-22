using GraphicMinimal;
using GraphicPanelWpf;
using LevelEditorGlobal;
using Splat;
using WpfControls.Controls.CameraSetting;

namespace Simulator.CameraTracking
{
    //Es darf genau ein LevelItem geben, was mit der Kamera verfolgt wird.
    //Wenn das Zentrum vom LevelItem weiter als DistanceToScreenCenter Cameraspace-Einheiten von der Bildschirmmitte entfernt ist, dann soll die Kamera sich so bewegen,
    //dass der LevelItem im Sichtfeld gehalten wird. Abhängig davon, wie sehr der CameraTracking-Punkt des LevelItems aus dem
    //DistanceToScreenCenter-Bereich raus geht, wird die Kamera per Federkraft beschleunigt. Die Feder bekommt ihre Steifigkeit 
    //über SpringConstant und ihre Reibung über AirFriction
    public class CameraTracker : ITimerHandler
    {
        private Camera2D camera;                        // Die Position/Zoom soll von dieser Kamera verändert werden
        private ICameraTrackedItem item;                // Dieser bewegbare Punkt soll immer im Sichtbereich bleiben
        private CameraTrackerData data;
        private Vector2D velocity = new Vector2D(0, 0); // Aktuelle Geschwindigkeit der Kamera
        private float mass = 1; //Masse der Kamera
        private RectangleF boundingBoxFromScene;


        public CameraTracker(Camera2D camera, ICameraTrackedItem item, CameraTrackerData data, RectangleF boundingBoxFromScene)
        {
            this.camera = camera;
            this.item = item;
            this.data = data;
            this.boundingBoxFromScene = data.MaxBorder != null ? data.MaxBorder.Value : boundingBoxFromScene;
            this.camera.Zoom = data.CameraZoom; //Zoom übernehmen

            var box = this.camera.GetScreenBox();
            if (data.DistanceToScreenCenter * 2 > box.Width)
                throw new Exception("The DistanceToScreenCenter must be less than the half camera-view-width");

            if (data.DistanceToScreenCenter * 2 > box.Height)
                throw new Exception("The DistanceToScreenCenter must be less than the half camera-view-height");

            SetTrackingPointToScreenCenter();
        }

        public void UpdateCameraTrackingItem(ICameraTrackedItem item)
        {
            if (item == null) throw new ArgumentNullException("item");

            this.item = item;
        }

        public bool IsActive 
        { 
            get => this.data.IsActive; 
            set                
            {
                if (this.data.IsActive != value)
                {
                    this.data.IsActive = value;
                    Reset();
                }
                
            }
        }

        public void Reset()
        {          
            this.camera.ShowOriginalPosition = false;

            if (this.data.IsActive)
            {
                this.camera.Zoom = data.CameraZoom;
                SetTrackingPointToScreenCenter();
            }
            else
            {
                this.camera.SetInitialCameraPosition();
            }
        }

        public void HandleTimerTick(float dt)
        {
            if (this.data.IsActive == false) return;

            var rec = this.item.BoundingBox;
            var box = this.camera.GetScreenBox();

            if (data.Mode == CameraTrackerData.TrackingMode.KeepAwayFromBorder)
            {
                //Messe Abstand zum Bildschirmrand
                float left = box.Left + this.data.DistanceToScreenBorder;
                float right = box.Right - this.data.DistanceToScreenBorder;
                float top = box.Top + this.data.DistanceToScreenBorder;
                float bottom = box.Bottom - this.data.DistanceToScreenBorder;

                //Federkraft wirken lassen wenn das Objekt am Rand ist
                if (rec.Left < left)
                {
                    this.velocity.X += GetSpringVelocityDelta(camera.LengthToScreen(left - rec.Left), dt);
                }
                if (rec.Right > right)
                {
                    this.velocity.X += GetSpringVelocityDelta(camera.LengthToScreen(right - rec.Right), dt);
                }
                if (rec.Top < top)
                {
                    this.velocity.Y += GetSpringVelocityDelta(camera.LengthToScreen(top - rec.Top), dt);
                }
                if (rec.Bottom > bottom)
                {
                    this.velocity.Y += GetSpringVelocityDelta(camera.LengthToScreen(bottom - rec.Bottom), dt);
                }
            }

            if (data.Mode == CameraTrackerData.TrackingMode.KeepInCenter)
            {
                //Messe Abstand zur Bildschirmmitte            
                var boxC = box.Center();
                float left = boxC.X - this.data.DistanceToScreenCenter;
                float right = boxC.X + this.data.DistanceToScreenCenter;
                float top = boxC.Y - this.data.DistanceToScreenCenter;
                float bottom = boxC.Y + this.data.DistanceToScreenCenter;

                //Federkraft wirken lassen wenn das Objekt am Rand ist
                var cen = rec.Center();
                if (cen.X < left)
                {
                    this.velocity.X += GetSpringVelocityDelta(camera.LengthToScreen(left - cen.X), dt);
                }
                if (cen.X > right)
                {
                    this.velocity.X += GetSpringVelocityDelta(camera.LengthToScreen(right - cen.X), dt);
                }
                if (cen.Y < top)
                {
                    this.velocity.Y += GetSpringVelocityDelta(camera.LengthToScreen(top - cen.Y), dt);
                }
                if (cen.Y > bottom)
                {
                    this.velocity.Y += GetSpringVelocityDelta(camera.LengthToScreen(bottom - cen.Y), dt);
                }
            }
            

            //Luftreibung wirkt immer
            this.velocity.X += GetAirFrictionVelocityDetla(this.velocity.X, dt);
            this.velocity.Y += GetAirFrictionVelocityDetla(this.velocity.Y, dt);

            this.camera.X += camera.LengthToCamera(this.velocity.X * dt);
            this.camera.Y += camera.LengthToCamera(this.velocity.Y * dt);

            camera.X = Math.Max(camera.X, this.boundingBoxFromScene.X);
            camera.Y = Math.Max(camera.Y, this.boundingBoxFromScene.Y);
            camera.X = Math.Min(camera.X, this.boundingBoxFromScene.Right - box.Width);
            camera.Y = Math.Min(camera.Y, this.boundingBoxFromScene.Bottom - box.Height);
        }

        private float GetSpringVelocityDelta(float positionError, float dt)
        {
            float springForce = -positionError * data.SpringConstant;


            float springVelocityDelta = springForce / this.mass * dt;

            return springVelocityDelta;
        }

        private float GetAirFrictionVelocityDetla(float velocity, float dt)
        {
            float frictionForce = -velocity * Math.Abs(velocity) * data.AirFriction * 0.1f - velocity * data.AirFriction;

            float frictionVelocityDelta = frictionForce / this.mass * dt; //Um diesen Betrag will die Reibung die Geschwindigkeit ändern
            float maxFrictionVelocityDelta = Math.Abs(velocity); //Sie darf aber maximal die Geschwindigkeit um diesen Wert ändern, um nicht über die Null zu springen
            frictionVelocityDelta = Math.Max(-maxFrictionVelocityDelta, Math.Min(maxFrictionVelocityDelta, frictionVelocityDelta)); //Clampe frictionVelocityDelta 
            return frictionVelocityDelta;
        }

        private void SetTrackingPointToScreenCenter()
        {
            var screenBox = this.camera.GetScreenBox();
            var screenCenter = new Vector2D(screenBox.X + screenBox.Width / 2, screenBox.Y + screenBox.Height / 2);

            var itemBox = this.item.BoundingBox;
            var itemCenter = new Vector2D(itemBox.X + itemBox.Width / 2, itemBox.Y + itemBox.Height / 2);
            var delta = screenCenter - itemCenter;
            camera.X -= delta.X;
            camera.Y -= delta.Y;
        }
    }
}
