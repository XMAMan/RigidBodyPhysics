using GraphicPanels;
using ReactiveUI;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Windows.Forms;
using Part1.Model.Simulator;
using System.IO;

namespace Part1.ViewModel.Simulator
{
    public class SimulatorViewModel : ReactiveObject, IGraphicPanelHandler, IShapeDataContainer, IActivateable
    {
        private bool isRunning = false;
        private readonly GraphicPanel2D panel;
        private Scene model;
        private System.Windows.Threading.DispatcherTimer timer;
        private string lastLoadedScene = string.Empty;

        [Reactive] public ImageSource PlayPauseImage { get; set; } = new BitmapImage(new Uri(FilePaths.PlayFile, UriKind.Absolute));

        public ReactiveCommand<Unit, Unit> RestartClick { get; private set; }
        public ReactiveCommand<Unit, Unit> PlayPauseClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SingleStepClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SwitchClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveClick { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadClick { get; private set; }
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
                    this.model.LoadShapeData(this.lastLoadedScene);
            });

            this.PlayPauseClick = ReactiveCommand.Create(() =>
            {
                this.isRunning = !this.isRunning;

                if (this.isRunning)
                {
                    this.PlayPauseImage = new BitmapImage(new Uri(FilePaths.PauseFile, UriKind.Absolute));
                }
                else
                {
                    this.PlayPauseImage = new BitmapImage(new Uri(FilePaths.PlayFile, UriKind.Absolute));
                }
            });

            this.SingleStepClick = ReactiveCommand.Create(() =>
            {
                this.model.TimeStep((float)timer.Interval.TotalMilliseconds);
            });

            this.SwitchClick = ReactiveCommand.Create(() =>
            {

            });

            this.SaveClick = ReactiveCommand.Create(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    File.WriteAllText(saveFileDialog.FileName, this.GetShapeData());
            });
            this.LoadClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    this.LoadShapeData(File.ReadAllText(openFileDialog.FileName));
            });
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (this.IsActivated == false) return;

            if (this.isRunning)
                this.model.TimeStep((float)timer.Interval.TotalMilliseconds);

            this.model.Draw(this.panel);
        }

        public void HandleMouseClick(MouseEventArgs e)
        {
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
        }

        public void HandleMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public void HandleMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
        }

        public void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
        }

        public string GetShapeData()
        {
            return this.model.GetShapeData();
        }

        public void LoadShapeData(string json)
        {
            this.model.LoadShapeData(json);
            this.lastLoadedScene = json;
        }
    }
}
