using ControlInterfaces;
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
using SimulatorControl.View.ManipulateJoint;
using SimulatorControl.ViewModel.ManipulateJoint;
using SimulatorControl.View;

namespace SimulatorControl.ViewModel
{
    public class SimulatorViewModel : ReactiveObject, IGraphicPanelHandler, IShapeDataContainer, IActivateable
    {
        private bool isRunning = false;
        private readonly GraphicPanel2D panel;
        private Scene model;
        private System.Windows.Threading.DispatcherTimer timer;
        private string lastLoadedScene = string.Empty;
        private StringBuilder stateLog = new StringBuilder();
        private bool shiftIsPressed = false;

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

        [Reactive] public System.Windows.Controls.UserControl ManipulateJointControl { get; set; }

        public bool ShowCollisionPoints
        {
            get => this.model.ShowCollisionPoints;
            set => model.ShowCollisionPoints = value;
        }
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


        public bool IsActivated { get; set; } = false;

        public SimulatorViewModel(GraphicPanel2D panel)
        {
            this.panel = panel;

            this.timer = new System.Windows.Threading.DispatcherTimer();
            this.timer.Interval = new TimeSpan(0, 0, 0, 0, 50);//50 ms
            this.timer.Tick += Timer_Tick;
            this.timer.Start();

            this.model = new Scene();

            this.RestartClick = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrEmpty(this.lastLoadedScene))
                    this.model = new Scene();
                else
                    this.LoadShapeData(this.lastLoadedScene);

                this.TimeStep = 0;
                this.stateLog = new StringBuilder();
            });

            this.PlayPauseClick = ReactiveCommand.Create(() =>
            {
                SetIsRunning(!this.isRunning);
            });

            this.SingleStepClick = ReactiveCommand.Create(() =>
            {
                DoSingleTimeStep();
            });

            this.SwitchClick = ReactiveCommand.Create(() =>
            {

            });

            this.SaveClick = ReactiveCommand.Create(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    File.WriteAllText(saveFileDialog.FileName, this.GetShapeData());
            });
            this.LoadClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    this.LoadShapeData(File.ReadAllText(openFileDialog.FileName));
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
                    this.model.BackgroundImage = BackgroundImageHelper.ClampImageSize(openFileDialog.FileName, this.panel.Width, this.panel.Height, "SimulatorBackgroundImage.bmp");
            });
            this.SaveImageClick = ReactiveCommand.Create(() =>
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    panel.GetScreenShoot().Save(saveFileDialog.FileName);
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

        private void DoSingleTimeStep()
        {
            stateLog.AppendLine(this.TimeStep + "\t: " + this.model.GetStateFromAllBodies());

            this.model.TimeStep((float)timer.Interval.TotalMilliseconds);
            this.TimeStep++;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (this.IsActivated == false) return;

            if (this.isRunning)
            {
                DoSingleTimeStep();
            }


            this.model.Draw(this.panel);


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

        public string GetShapeData()
        {
            return this.model.GetShapeData();
        }

        public void LoadShapeData(string json)
        {
            this.model.LoadShapeData(json);
            this.lastLoadedScene = json;

            this.ManipulateJointControl = new ManipulateJointControl() { DataContext = new ManipulateJointViewModel(this.model.GetAllJoints()) };
        }
    }
}
