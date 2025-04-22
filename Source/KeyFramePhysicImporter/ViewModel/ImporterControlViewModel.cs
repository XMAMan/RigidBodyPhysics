using GraphicPanels;
using GraphicPanelWpf;
using JsonHelper;
using KeyFrameGlobal;
using KeyFramePhysicImporter.Model.PhysicSceneDrawing;
using KeyFramePhysicImporter.Model;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics;
using System.Reactive;
using WpfControls.Controls.CameraSetting;
using DynamicData;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace KeyFramePhysicImporter.ViewModel
{
    //Zeigt die PhysicScene an. Hier kann man auswählen, welche Objekte fix sein sollen. 
    public class ImporterControlViewModel : ReactiveObject, IImporterControl, IGraphicPanelHandler, ITimerHandler
    {
        private GraphicPanel2D panel;
        private PhysicSceneDrawer drawer;
        private PhysicScene scene;
        private Camera2D camera;

        private enum MouseState
        {
            Nothing,        //Mauszeiger macht nicht, wenn ins Panel geklickt wird
            MakeFix,        //RigidBody wird fix, wenn er angeklickt wird
            DefineColor,    //RigidBody bekommt die selektierte Farbe zugewiesen
        }
        private MouseState mouseState = MouseState.Nothing;
        private Color selectedColor = Color.Black;

        [Reactive] public bool HasGravity { get; set; } = false;
        public ReactiveCommand<Unit, Unit> ImportClick { get; private set; }
        public ReactiveCommand<Unit, Unit> MakeFixClick { get; private set; }
        public ReactiveCommand<Unit, Unit> DefineColor1Click { get; private set; }
        public ReactiveCommand<Unit, Unit> DefineColor2Click { get; private set; }
        public ReactiveCommand<Unit, Unit> DefineColor3Click { get; private set; }

        [Reactive] public System.Windows.Media.SolidColorBrush MakeFixBorderColor { get; set; } = System.Windows.Media.Brushes.Transparent;
        [Reactive] public System.Windows.Media.SolidColorBrush Color1BorderColor { get; set; } = System.Windows.Media.Brushes.Transparent;
        [Reactive] public System.Windows.Media.SolidColorBrush Color2BorderColor { get; set; } = System.Windows.Media.Brushes.Transparent;
        [Reactive] public System.Windows.Media.SolidColorBrush Color3BorderColor { get; set; } = System.Windows.Media.Brushes.Transparent;

        [Reactive] public System.Windows.Media.SolidColorBrush Color1 { get; set; } = System.Windows.Media.Brushes.LightGray;
        [Reactive] public System.Windows.Media.SolidColorBrush Color2 { get; set; } = System.Windows.Media.Brushes.Gray;
        [Reactive] public System.Windows.Media.SolidColorBrush Color3 { get; set; } = System.Windows.Media.Brushes.Black;


        public ImporterControlViewModel(GraphicPanel2D panel, string physicSceneJson)
        {
            this.panel = panel;
            this.scene = StringToPhysicScene(physicSceneJson);

            this.drawer = new PhysicSceneDrawer(scene, panel);
            this.camera = new Camera2D(panel.Width, panel.Height, this.drawer.GetBoundingBoxFromScene());

            this.ImportClick = ReactiveCommand.Create(() =>
            {
                this.ImportIsFinished(GetAnimationDataFromPhysicScene(this.scene));
            });

            this.MakeFixClick = ReactiveCommand.Create(() =>
            {
                this.mouseState = MouseState.MakeFix;
                ClearBorders();
                this.MakeFixBorderColor = System.Windows.Media.Brushes.Red;
            });

            this.DefineColor1Click = ReactiveCommand.Create(() =>
            {
                this.mouseState = MouseState.DefineColor;
                this.selectedColor = Color1.ToDrawingColor();
                ClearBorders();
                this.Color1BorderColor = System.Windows.Media.Brushes.Red;
            });
            this.DefineColor2Click = ReactiveCommand.Create(() =>
            {
                this.mouseState = MouseState.DefineColor;
                this.selectedColor = Color2.ToDrawingColor();
                ClearBorders();
                this.Color2BorderColor = System.Windows.Media.Brushes.Red;
            });
            this.DefineColor3Click = ReactiveCommand.Create(() =>
            {
                this.mouseState = MouseState.DefineColor;
                this.selectedColor = Color3.ToDrawingColor();
                ClearBorders();
                this.Color3BorderColor = System.Windows.Media.Brushes.Red;
            });
        }

        private void ClearBorders()
        {
            this.MakeFixBorderColor = System.Windows.Media.Brushes.Transparent;
            this.Color1BorderColor = System.Windows.Media.Brushes.Transparent;
            this.Color2BorderColor = System.Windows.Media.Brushes.Transparent;
            this.Color3BorderColor = System.Windows.Media.Brushes.Transparent;
        }

        private static PhysicScene StringToPhysicScene(string physicSceneJson)
        {
            var rawData = Helper.FromCompactJson<PhysicSceneExportData>(physicSceneJson);
            PhysicScene scene = new PhysicScene(rawData) { HasGravity = false };

            return scene;
        }

        private AnimatorInputData GetAnimationDataFromPhysicScene(PhysicScene physicScene)
        {
            var properties = PhysicSceneAnimationPropertyConverter.Convert(physicScene.GetAllPublicData());
            physicScene.HasGravity = this.HasGravity;

            return new AnimatorInputData()
            {
                Properties = properties,
                AnimationObject = new PhysicSceneAnimationObject(physicScene),
                AnimationModelDrawer = this.drawer //Der Drawer speichert die Information, welche Farben die RigidBodys haben
            };
        }

        public AnimatorInputData GetAnimationData()
        {
            return GetAnimationDataFromPhysicScene(this.scene);
        }

        #region IImporterControl
        public event ImportIsFinishedHandler ImportIsFinished;
        #endregion

        #region ITimerHandler

        public void HandleTimerTick(float dt)
        {
            panel.ClearScreen(System.Drawing.Color.White);
            this.drawer.Draw(this.camera);
            panel.FlipBuffer();
        }
        #endregion

        #region IGraphicPanelHandler
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            if (this.mouseState == MouseState.MakeFix)
            {
                var objectSpacePoint = this.camera.PointToCamera(new PointF(e.X, e.Y));
                var data = this.scene.TryToGetBodyWithMouseClick(objectSpacePoint.ToPhx());
                if (data != null)
                {
                    this.scene = SwitchMassPropertyFromRigidBody(this.scene, data.RigidBody);
                    this.drawer.ReloadNewPhysicScene(this.scene);
                }
            }

            if (this.mouseState == MouseState.DefineColor)
            {
                var objectSpacePoint = this.camera.PointToCamera(new PointF(e.X, e.Y));
                var data = this.scene.TryToGetBodyWithMouseClick(objectSpacePoint.ToPhx());
                if (data != null)
                {
                    this.drawer.SetColorFromRigidBody(data.RigidBody, this.selectedColor);
                }
            }
        }

        private static PhysicScene SwitchMassPropertyFromRigidBody(PhysicScene scene, IPublicRigidBody body)
        {
            int index = scene.GetAllBodys().IndexOf(body);
            var export = scene.GetExportData();
            var m = export.Bodies[index].MassData;
            m.Type = NegateMassType(m.Type);

            PhysicScene newScene = new PhysicScene(export) { HasGravity = false };

            return newScene;
        }

        private static RigidBodyPhysics.ExportData.RigidBody.MassData.MassType NegateMassType(RigidBodyPhysics.ExportData.RigidBody.MassData.MassType type)
        {
            if (type == RigidBodyPhysics.ExportData.RigidBody.MassData.MassType.Infinity)
                return RigidBodyPhysics.ExportData.RigidBody.MassData.MassType.Density;
            else
                return RigidBodyPhysics.ExportData.RigidBody.MassData.MassType.Infinity;
        }

        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public void HandleMouseEnter()
        {
        }
        public void HandleMouseLeave()
        {
        }
        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
        }
        public void HandleSizeChanged(int width, int height)
        {
            this.camera.UpdateScreenSize(width, height);
            this.camera.SetInitialCameraPosition();
        }

        #endregion

        #region Export/Import des Memento

        public ImporterControlExportData GetExportData()
        {
            var data = new ImporterControlExportData()
            {
                HasGravity = this.HasGravity,
                IsFix = this.scene.GetExportData().Bodies.Select(x => x.MassData.Type == RigidBodyPhysics.ExportData.RigidBody.MassData.MassType.Infinity).ToArray(),
                Colors = this.drawer.GetColors(),
            };

            return data;
        }
        public void LoadFromExportData(ImporterControlExportData data)
        {
            this.scene = SetIsFixFromPhysicScene(this.scene, data.IsFix);
            this.HasGravity = this.scene.HasGravity = data.HasGravity;
            this.drawer.ReloadNewPhysicScene(this.scene);

            var bodys = this.scene.GetAllBodys();
            for (int i = 0; i < bodys.Length; i++)
            {
                this.drawer.SetColorFromRigidBody(bodys[i], data.Colors[i]);
            }

        }

        //Da IsFix ist eine Readonly-Eigenschaft, wird hier eine neue PhysicScene erzeugt, welche die gewünschten IsFix-Werte hat
        private static PhysicScene SetIsFixFromPhysicScene(PhysicScene physicScene, bool[] isFix)
        {
            var export = physicScene.GetExportData();
            for (int i = 0; i < isFix.Length; i++)
            {
                var body = export.Bodies[i];
                if (IsFix(body.MassData.Type) != isFix[i])
                {
                    body.MassData.Type = NegateMassType(body.MassData.Type);
                }
            }

            PhysicScene newScene = new PhysicScene(export) { HasGravity = false };

            return newScene;
        }

        private static bool IsFix(RigidBodyPhysics.ExportData.RigidBody.MassData.MassType type)
        {
            return type == RigidBodyPhysics.ExportData.RigidBody.MassData.MassType.Infinity;
        }

        #endregion
    }
}
