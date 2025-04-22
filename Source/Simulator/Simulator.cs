using Simulator.CameraTracking;
using GraphicPanels;
using KeyFrameGlobal;
using LevelEditorGlobal;
using PhysicSceneDrawing;
using RigidBodyPhysics;
using WpfControls.Controls.CameraSetting;
using Simulator.Animation;
using TextureEditorGlobal;
using Simulator.ForceTracking;
using GraphicMinimal;
using RigidBodyPhysics.ExportData;

namespace Simulator
{
    //Input: Alle PhysicScene- und Polygon-Objekte vom Editor; Alle BackgroundItems
    //Die Kamera wird bei diesen Simulator hier von außen reingeben, damit man im Leveleditor während der Simulation die Kameraposition
    //über das SmallWindow steuern kann
    public class Simulator : ILeveleditorUsedSimulator
    {
        protected SimulatorInputData inputData;
        protected PhysicScene physicScene;
        protected List<RuntimeLevelItem> levelItems = new List<RuntimeLevelItem>(); //Verweist auf IPublic-Objekte aus der PhysicScene

        protected PhysicSceneDrawer sceneDrawer;
        private SmallWindow smallWindow;

        protected LevelItemAnimator animator;
        protected KeyboardControl.LevelItemKeyboardHandler keyHandler;

        protected Camera2D camera;
        protected CameraTracker cameraTracker = null;//Steuert die Kameraposition so dass ein Objekt immer im Sichtbereich bleibt

        protected BackgroundItemSimulatorExportData[] backgroundItems;

        public ImageData BackgroundImage { get; protected set; }
        public bool ShowSmallWindow { get; set; } = true;

        

        //Wird vom Leveleditor verwendet (Kamera wird von außen über das CameraViewModel gesteuert)
        public Simulator(SimulatorInputData data, Size panelSize, Camera2D camera, float timerIntercallInMilliseconds)
        {
            this.inputData = data;

            ExportToRuntimeConverter.Convert(data.PhysicLevelItems, data.CollisionMatrix, out this.physicScene, out RuntimeLevelItem[] startLevelItems);
            this.levelItems.AddRange(startLevelItems);

            this.physicScene.HasGravity = data.HasGravity;
            this.physicScene.DoPositionalCorrection = true;
            this.physicScene.IterationCount = data.IterationCount;
            this.physicScene.Gravity = data.Gravity; //Defaultwert ist 0.001

            this.sceneDrawer = new PhysicSceneDrawer(this.physicScene, new VisualisizerOutputData(data.PhysicLevelItems.SelectMany(x => x.TextureData.Textures).ToArray()));
            this.smallWindow = new SmallWindow(panelSize.Width, panelSize.Height, camera);

            this.camera = camera;

            this.animator = new LevelItemAnimator(timerIntercallInMilliseconds);
            this.keyHandler = new KeyboardControl.LevelItemKeyboardHandler();
            for (int i=0;i<startLevelItems.Length;i++)
            {
                Animator[] animators = null;

                var animationData = data.PhysicLevelItems[i].AnimationData;
                if (animationData != null && animationData.Length > 0)
                {
                    this.animator.AddLevelItem(startLevelItems[i], data.PhysicLevelItems[i].AnimationData);
                    animators = this.animator.GetAnimationRuntimDataFromLevelItem(startLevelItems[i]);
                }

                if (data.PhysicLevelItems[i].KeyboardMappings != null && data.PhysicLevelItems[i].KeyboardMappings.Length > 0)
                {
                    this.keyHandler.AddLevelItem(startLevelItems[i], animators, data.PhysicLevelItems[i].KeyboardMappings);
                }                
            }

            if (data.CameraTrackedLevelItemId != -1 && data.CameraTrackerData.IsActive && camera.ShowOriginalPosition == false)
            {
                var levelItem = this.levelItems.First(x => x.LevelItemId == data.CameraTrackedLevelItemId);
                var cameraTrackedBodys = new CameraTrackedRigidBodys(levelItem.Bodies);
                camera.UpdateSceneBoundingBox(this.sceneDrawer.GetPhysicBoundingBoxFromScene());
                this.cameraTracker = new CameraTracker(camera, cameraTrackedBodys, data.CameraTrackerData, this.sceneDrawer.GetPhysicBoundingBoxFromScene());
                this.cameraTracker.IsActive = true;
                this.CameraModus = CameraMode.CameraTracker;
            }

            this.BackgroundImage = data.BackgroundImage;
            this.backgroundItems = data.BackgroundItems;

            this.FixCameraArea = new RectangleF(0, 0, panelSize.Width, panelSize.Height);

            if (data.BackgroundImage.Mode == ImageMode.StretchWithAspectRatio && string.IsNullOrEmpty(data.BackgroundImage.FileName) == false)
            {
                this.camera.UpdateBackgroundImage(data.BackgroundImage.Size);
                this.FixCameraArea = new RectangleF(0, 0, data.BackgroundImage.Size.Width, data.BackgroundImage.Size.Height); //2D-Spiel, wo man von oben drauf schaut (Bsp: CarDrifter)
                this.CameraModus = CameraMode.FixArea;                
            }
            if (data.BackgroundImage.Mode == ImageMode.NoStretch)
            {
                this.CameraModus = CameraMode.Pixel;
            }
        }

        #region Camera-Handling
        public enum CameraMode
        {
            None,            //Initialwert
            Pixel,           //1 Pixel = 1 Kameraeinheit; Sichtfenster entspricht panel.Width/Height
            SceneBoundingBox,//Es wird die gesamte Scene angezeigt
            CameraTracker,   //Es wird das LevelItem mit der ID CameraTrackedLevelItemId mit der Kamera verfolgt
            FixArea,         //Es wird die FixCameraArea angezeigt
        }

        private CameraMode cameraMode = CameraMode.None;
        public CameraMode CameraModus
        {
            get => this.cameraMode;
            set
            {
                if (value != this.cameraMode)
                {
                    this.cameraMode = value;
                    SetCameraMode(value);
                }                
            }
        }

        private void SetCameraMode(CameraMode cameraMode)
        {
            switch(cameraMode)
            {
                case CameraMode.None:
                    throw new Exception("Please do not use this mode!");

                case CameraMode.Pixel:
                    this.IsCameraTrackerActive = false;
                    this.camera.ShowOriginalPosition = true;
                    break;

                case CameraMode.SceneBoundingBox:
                    this.IsCameraTrackerActive = false;
                    this.camera.ShowOriginalPosition = false;
                    this.camera.InitialPosition = Camera2D.InitialPositionIfAutoZoomIsActivated.SceneCenterToScreenCenter;
                    this.camera.UpdateSceneBoundingBox(this.sceneDrawer.GetPhysicBoundingBoxFromScene());
                    this.camera.SetInitialCameraPosition();
                    break;

                case CameraMode.CameraTracker:
                    this.IsCameraTrackerActive = true; 
                    break;

                case CameraMode.FixArea:
                    this.IsCameraTrackerActive = false;
                    this.camera.ShowOriginalPosition = false;
                    this.camera.UpdateSceneBoundingBox(this.FixCameraArea);
                    this.camera.InitialPosition = Camera2D.InitialPositionIfAutoZoomIsActivated.ToLeftTopCorner;
                    this.camera.SetInitialCameraPosition();
                    
                    break;
            }
        }

        public RectangleF FixCameraArea { get; set; } //Dieser Bereich wird angezeigt, wenn der Kamera-Modus FixArea verwendet wird

        public RectangleF GetScreenBox()
        {
            return this.camera.GetScreenBox();
        }

        private bool IsCameraTrackerActive
        {
            get
            {
                if (this.cameraTracker == null) return false;

                return this.cameraTracker.IsActive;
            }
            set
            {
                if (this.cameraTracker != null)
                {
                    this.cameraTracker.IsActive = value;
                }                
            }
        }

        private int cameraTrackedLevelItemId;
        public int CameraTrackedLevelItemId
        {
            get
            {
                return cameraTrackedLevelItemId;
            }
            set
            {
                this.cameraTrackedLevelItemId = value;

                var levelItem = this.levelItems.First(x => x.LevelItemId == this.cameraTrackedLevelItemId);
                var cameraTrackedBodys = new CameraTrackedRigidBodys(levelItem.Bodies);
                this.cameraTracker.UpdateCameraTrackingItem(cameraTrackedBodys);
            }
        }
        #endregion

        public virtual void PanelSizeChangedHandler(int width, int height)
        {
            this.camera.UpdateScreenSize(width, height);

            
            switch (this.CameraModus)
            {
                case CameraMode.None:
                case CameraMode.Pixel:
                    //Do nothing
                    break;
                case CameraMode.SceneBoundingBox:
                    this.camera.SetInitialCameraPosition();
                    break;
                case CameraMode.CameraTracker:
                    if (this.cameraTracker != null)
                    {
                        this.cameraTracker.Reset();
                    }
                    break;
                case CameraMode.FixArea:
                    this.camera.UpdateSceneBoundingBox(this.FixCameraArea);
                    this.camera.SetInitialCameraPosition();
                    this.camera.X = 0;
                    this.camera.Y = 0;
                    break;
            }
                     

            this.smallWindow.HandleSizeChanged(width, height);
        }

        public virtual void MoveOneStep(float dt)
        {
            this.animator.HandleTimerTick(dt); //Sollwerte der Gelenkwerte bewegen

            this.physicScene.TimeStep(dt); //Ist-Werte der Gelenke/Körper bewegen

            if (this.cameraTracker != null)
            {
                this.cameraTracker.HandleTimerTick(dt); //Kamera-Position bewegen
            }
        }

        public virtual void Draw(GraphicPanel2D panel)
        {
            panel.ClearScreen(Color.White);
            DrawBackground(panel);
            SetCameraMatrix(panel);
            DrawPhysicItems(panel, false);
            DrawBackgroundItems(panel);
            DrawSmallWindow(panel);            
        }

        public virtual void SetCameraMatrix(GraphicPanel2D panel)
        {
            panel.MultTransformationMatrix(camera.GetPointToSceenMatrix());
        }

        public virtual void DrawPhysicItemBorders(GraphicPanel2D panel, Pen borderPen)
        {
            this.sceneDrawer.DrawPhysicBorder(panel, borderPen);
        }

        public virtual void DrawTextureBorders(GraphicPanel2D panel, Pen borderPen)
        {
            this.sceneDrawer.DrawTextureBorder(panel, borderPen);
        }

        public virtual void DrawDistanceJoints(GraphicPanel2D panel, Pen borderPen)
        {
            this.sceneDrawer.DrawDistanceJoints(panel, borderPen);
        }

        public virtual void DrawBackground(GraphicPanel2D panel)
        {
            if (string.IsNullOrEmpty(this.BackgroundImage.FileName) == false)
            {
                var s = this.BackgroundImage.Size;

                switch (this.BackgroundImage.Mode)
                {
                    case ImageMode.StretchWithoutAspectRatio:
                        panel.DrawFillRectangle(this.BackgroundImage.FileName, 0, 0, panel.Width, panel.Height, false, Color.White);
                        break;

                    case ImageMode.StretchWithAspectRatio:
                        float factor = Camera2D.GetScaleFactor(panel.Size, s);
                        panel.DrawFillRectangle(this.BackgroundImage.FileName, 0, 0, s.Width * factor, s.Height * factor, false, Color.White);
                        break;

                    case ImageMode.NoStretch:
                        panel.DrawFillRectangle(this.BackgroundImage.FileName, 0, 0, s.Width, s.Height, false, Color.White);
                        break;
                }
                
            }
                
        }

        public virtual void DrawPhysicItems(GraphicPanel2D panel, bool drawDistanceJoints)
        {
            this.sceneDrawer.Draw(panel);

            if (drawDistanceJoints )
            {
                this.sceneDrawer.DrawDistanceJoints(panel);
            }
        }

        //Testausgabe der Kollisionspunkte
        public virtual void DrawCollisionPoints(GraphicPanel2D panel)
        {
            panel.DisableDepthTesting();

            var matrix = panel.GetTransformationMatrix();
            float sizeFactor = 1.0f / Matrix4x4.GetSizeFactorFromMatrix(matrix);

            var points = this.physicScene.GetCollisions();
            foreach (var point in points)
            {
                //panel.DrawCircleWithLines(Pens.Red, point.Start.ToGrx(), 80 * sizeFactor, 10);
                panel.DrawLine(new Pen(Color.Red, 2), point.End.ToGrx(), (point.End - point.Normal * 20 * sizeFactor).ToGrx());
            }
        }

        public virtual void DrawSmallWindow(GraphicPanel2D panel)
        {
            if (this.ShowSmallWindow)
            {
                this.smallWindow.Draw(panel, (frontColor, backColor) => this.sceneDrawer.DrawWithTwoColors(panel, frontColor, backColor));
            }            
        }

        public virtual void DrawBackgroundItems(GraphicPanel2D panel)
        {
            if (this.backgroundItems == null) return;

            foreach (var item in this.backgroundItems)
            {
                if (string.IsNullOrEmpty(item.TextureFile) == false)
                    panel.DrawFillRectangle(item.TextureFile, item.Center.Xi, item.Center.Yi, (int)item.Width, (int)item.Height, true, Color.White, item.AngleInDegree);
                else
                    panel.DrawFillRectangle(Color.Green, item.Center.Xi, item.Center.Yi, (int)item.Width, (int)item.Height, item.AngleInDegree);
            }
        }

        public virtual void HandleMouseDown(MouseEventArgs e)
        {
            //Kamera-Position verändern
            if (this.smallWindow.HandleMouseDown(e)) return;

            //Physik-Objekt anklicken und mit der Maus bewegen
            var mousePosition = this.camera.PointToCamera(new PointF(e.X, e.Y)).ToPhx();
            var data = this.physicScene.TryToGetBodyWithMouseClick(mousePosition);
            if (data != null)
            {
                var mouseData = RigidBodyPhysics.MouseBodyClick.MouseConstraintUserData.CreateWithoutDamping();
                this.physicScene.SetMouseConstraint(data, mouseData);
            }
        }

        public virtual void HandleMouseUp(MouseEventArgs e)
        {
            this.smallWindow.HandleMouseUp(e); //Kamera nicht mehr weiter bewegen

            this.physicScene.ClearMouseConstraint(); //Angeklicktes Physikobjekt loslassen
        }

        public virtual void HandleMouseMove(MouseEventArgs e)
        {
            if (this.smallWindow.HandleMouseMove(e)) return;

            var mousePosition = this.camera.PointToCamera(new PointF(e.X, e.Y)).ToPhx();
            this.physicScene.UpdateMousePosition(mousePosition);
        }

        public virtual void HandleMouseWheel(MouseEventArgs e)
        {
            this.smallWindow.HandleMouseWheel(e);
        }

        public virtual void HandleKeyDown(System.Windows.Input.Key key)
        {
            this.keyHandler.HandleKeyDown(key);
        }
        public virtual void HandleKeyUp(System.Windows.Input.Key key)
        {
            this.keyHandler.HandleKeyUp(key);
        }

        public RectangleF GetBoundingBoxFromScene()
        {
            return this.sceneDrawer.GetPhysicBoundingBoxFromScene();
        }

        public ForceTracker CreateForceTracker()
        {
            return new ForceTracker(this.physicScene);
        }

        public PhysisSceneIndexDrawer CreatePhysisSceneIndexDrawer(PhysisSceneIndexDrawerSettings forceTrackerDrawingSettings)
        {
            return new PhysisSceneIndexDrawer(this.physicScene, forceTrackerDrawingSettings);
        }

        public PhysicSceneExportData GetPhysicSceneExportData()
        {
            return this.physicScene.GetExportData();
        }
    }
}
