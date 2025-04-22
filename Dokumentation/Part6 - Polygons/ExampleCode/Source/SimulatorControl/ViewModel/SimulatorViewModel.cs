using GraphicPanels;
using PhysicEngine.MathHelper;
using PhysicEngine;
using SimulatorControl.Model;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.IO;
using SimulatorControl.ViewModel.ManipulateJoint;
using SimulatorControl.View;
using GraphicPanelWpf;
using WpfControls.Model;
using PhysicEngine.ExportData;

namespace SimulatorControl.ViewModel
{
    public class SimulatorViewModel : ReactiveObject, IGraphicPanelHandler, IStringSerializable, ITimerHandler
    {
        private bool isRunning = false;
        private readonly GraphicPanel2D panel;
        private Scene model;
        
        private string lastLoadedScene = string.Empty;
        private StringBuilder stateLog = new StringBuilder();
        private bool shiftIsPressed = false;

        private PrintSettingsWindow printSettingsWindow = null; //Damit verhinder ich, dass der Dialog zweimal geöffnet wird
        private PrintSettings printSettings = new PrintSettings();

        [Reactive] public ImageSource PlayPauseImage { get; set; } = new BitmapImage(new Uri(FilePaths.PlayFile, UriKind.Absolute));

        public ReactiveCommand<Unit, Unit> RestartClick { get; private set; }
        public ReactiveCommand<Unit, Unit> PlayPauseClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SingleStepClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SwitchClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadClick { get; private set; }
        public ReactiveCommand<Unit, Unit> PushBodysApartClick { get; private set; }
        public ReactiveCommand<Unit, Unit> CopyLogToClipboard { get; private set; }
        public ReactiveCommand<Unit, Unit> ChangeBackgroundClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveImageClick { get; private set; }
        public ReactiveCommand<Unit, Unit> PrintSettingsClick { get; private set; }
        [Reactive] public bool ShowSaveLoadButtons { get; private set; }

        [Reactive] public System.Windows.Controls.UserControl ManipulateJointControl { get; set; }

        public bool DoPositionalCorrection
        {
            get => this.model.DoPositionalCorrection;
            set => model.DoPositionalCorrection = value;
        }
        public bool DoWarmStart
        {
            get => this.model.DoWarmStart;
            set => model.DoWarmStart = value;
        }
        public bool HasGravity
        {
            get => this.model.HasGravity;
            set => model.HasGravity = value;
        }

        public PhysicScene.SolverType SolverType
        {
            get => this.model.SolverType;
            set => this.model.SolverType = value;
        }
        public int IterationCount
        {
            get => this.model.IterationCount;
            set => this.model.IterationCount = value;
        }

        public IEnumerable<PhysicScene.SolverType> SolverTypeValues
        {
            get
            {
                return Enum.GetValues(typeof(PhysicScene.SolverType))
                    .Cast<PhysicScene.SolverType>();
            }
        }

        [Reactive] public int TimeStep { get; set; } = 0;
        public int MaxTimeStep { get; set; } = 100;
        public bool UseMaxTimeStep { get; set; } = false;

        public SimulatorViewModel(GraphicPanel2D panel, float dt, bool showSaveLoadButtons)
        {
            this.panel = panel;
            this.ShowSaveLoadButtons = showSaveLoadButtons;

            this.model = new Scene();

            this.RestartClick = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrEmpty(this.lastLoadedScene))
                    this.model = new Scene();
                else
                    this.LoadFromExportString(this.lastLoadedScene);

                this.TimeStep = 0;
                this.stateLog = new StringBuilder();
            });

            this.PlayPauseClick = ReactiveCommand.Create(() =>
            {
                SetIsRunning(!this.isRunning);
            });

            this.SingleStepClick = ReactiveCommand.Create(() =>
            {
                DoSingleTimeStep(dt);
            });

            this.SwitchClick = ReactiveCommand.Create(() =>
            {

            });

            this.SaveClick = ReactiveCommand.Create(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    File.WriteAllText(saveFileDialog.FileName, this.GetExportString());
            });
            this.LoadClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    this.LoadFromExportString(File.ReadAllText(openFileDialog.FileName));
            });
            this.PushBodysApartClick = ReactiveCommand.Create(() =>
            {
                this.model.PushBodysApart();
            });
            this.CopyLogToClipboard = ReactiveCommand.Create(() =>
            {
                System.Windows.Clipboard.SetText(this.stateLog.ToString());
            });
            this.ChangeBackgroundClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Filter = "png files (*.png)|*.png|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.panel.CreateOrUpdateNamedBitmapTexture("BackgroundImage", new Bitmap(openFileDialog.FileName));
                }
                    
            });
            this.SaveImageClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    panel.GetScreenShoot().Save(saveFileDialog.FileName);
            });

            this.PrintSettingsClick = ReactiveCommand.Create(() =>
            {
                if (this.printSettingsWindow != null) this.printSettingsWindow.Close();

                this.printSettingsWindow = new PrintSettingsWindow() { DataContext = this.printSettings };
                this.printSettingsWindow.Show();
            });
        }

        private void SetIsRunning(bool state)
        {
            this.isRunning = state;

            if (this.isRunning)
            {
                this.PlayPauseImage = new BitmapImage(new Uri(FilePaths.PauseFile, UriKind.Absolute));
            }
            else
            {
                this.PlayPauseImage = new BitmapImage(new Uri(FilePaths.PlayFile, UriKind.Absolute));
            }
        }

        private void DoSingleTimeStep(float dt)
        {
            stateLog.AppendLine(this.TimeStep + "\t: " + this.model.GetStateFromAllBodies());

            this.model.TimeStep(dt);
            this.TimeStep++;
        }

        public void HandleTimerTick(float dt)
        {
            if (this.isRunning)
            {
                DoSingleTimeStep(dt);
            }


            panel.ClearScreen(System.Drawing.Color.White);
            if (panel.IsNamedBitmapTextureAvailable("BackgroundImage"))
            {
                var s = panel.GetTextureSize("BackgroundImage");
                panel.DrawFillRectangle("BackgroundImage", 0, 0, s.Width, s.Height, false, System.Drawing.Color.White);
            }


            this.model.Draw(this.panel, this.printSettings);
            panel.FlipBuffer();

            if (this.UseMaxTimeStep && this.TimeStep >= this.MaxTimeStep)
            {
                SetIsRunning(false);
            }
        }

        public void HandleMouseClick(MouseEventArgs e)
        {
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            this.model.UpdateMousePosition(new Vec2D(e.X, e.Y));
        }

        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            var mousePosition = new Vec2D(e.X, e.Y);
            var data = this.model.TryToGetBodyWithMouseClick(mousePosition);
            if (data != null)
            {
                var mouseData = this.shiftIsPressed ? PhysicEngine.MouseBodyClick.MouseConstraintUserData.CreateWithDamping() : PhysicEngine.MouseBodyClick.MouseConstraintUserData.CreateWithoutDamping();
                this.model.SetMouseConstraint(data, mouseData);
            }
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            this.model.ClearMouseConstraint();
        }
        public void HandleMouseEnter() { }
        public void HandleMouseLeave() { }
        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = true;
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
                this.shiftIsPressed = false;
        }

        public object GetExportObject()
        {
            return this.model.GetExportObject();
        }
        public void LoadFromExportObject(object exportObject)
        {
            this.model.LoadFromExportObject(exportObject);
            this.lastLoadedScene = JsonHelper.Helper.ToJson((PhysicSceneExportData)exportObject);
            this.ManipulateJointControl = new ManipulateJointControl() { DataContext = new ManipulateJointViewModel(this.model.GetAllJoints(), this.model.GetAllThrusters(), this.model.GetAllRotaryMotors()) };
        }

        public string GetExportString()
        {
            return this.model.GetExportString();
        }

        public void LoadFromExportString(string json)
        {
            this.model.LoadFromExportString(json);
            this.lastLoadedScene = json;

            this.ManipulateJointControl = new ManipulateJointControl() { DataContext = new ManipulateJointViewModel(this.model.GetAllJoints(), this.model.GetAllThrusters(), this.model.GetAllRotaryMotors()) };
        }

        public void HandleSizeChanged(int width, int height)
        {
        }
    }
}
