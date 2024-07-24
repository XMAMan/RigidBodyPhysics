using LevelEditorGlobal;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Reactive.Linq;
using LevelEditorControl.EditorFunctions;
using System;
using System.Windows.Forms;
using Simulator.ForceTracking;

namespace LevelEditorControl.Controls.SimulatorControl
{
    internal class SimulatorViewModel : ReactiveObject
    {
        [Reactive] public ImageSource PlayPauseImage { get; set; } = new BitmapImage(new Uri(FilePaths.PlayFile, UriKind.Absolute));

        public ReactiveCommand<Unit, Unit> RestartClick { get; private set; }
        public ReactiveCommand<Unit, Unit> PlayPauseClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SingleStepClick { get; private set; }
        public ReactiveCommand<Unit, Unit> ReplayClick { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveReplayClick { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadReplayClick { get; private set; }
        public ReactiveCommand<Unit, Unit> CopyPhysicSceneToClipboard { get; private set; }        
        public ReactiveCommand<Unit, Unit> ShowForceDataClick { get; private set; }


        [Reactive] public SimulatorState State { get; set; } = SimulatorState.Inactive;
        [Reactive] public bool HasGravity { get; set; } = true;
        [Reactive] public int IterationCount { get; set; } = 50;
        [Reactive] public float Gravity { get; set; } = 0.001f;

        [Reactive] public bool SimulatorIsActive { get; set; } = false;
        [Reactive] public bool ShowForceDataButton { get; set; } = false;

        [Reactive] public bool ShowReplayButton { get; set; } = false;
        [Reactive] public string LoadedReplayFile { get; set; }
        [Reactive] public string TimerTickCounter { get; set; }
        public bool UseCameraTracking
        {
            get => this.cameraTrackerData.IsActive;
            set
            {
                this.cameraTrackerData.IsActive = value;
                this.RaisePropertyChanged(nameof(UseCameraTracking));
            }
        }

        public bool ShowForceData
        {
            get => this.editorState.ShowForceData;
            set
            {
                this.editorState.ShowForceData = value;
                this.RaisePropertyChanged(nameof(ShowForceData));
            }
        }

        [Reactive] public bool UseCameraTrackingCheckboxIsVisible { get; set; }

        private SimulatorFunction model = null; //Das model kann null sein, weil der Editor erst dann in den SimulatorState geht, wenn jemand auf ein Button von hier drückt
        private bool isRunning = false;

        private string lastLoadedFileName = null;
        private CameraTrackerData cameraTrackerData;
        private EditorState editorState;

        public enum SimulatorState
        {
            Inactive,  //this.model == null
            Restarted, //Animation läuft noch nicht und er steht am Anfang
            Running,   //Simulation läuft und man darf mit der Tastatur die Scene steuern
            Paused,    //Animation war kurz mal auf Running und dann hat jemand die Pausetaste gedrückt
            Replay,    //Replay wird abgespielt
        }

        public PhysisSceneIndexDrawerSettings ForceTrackerDrawingSettings { get; private set; } = new PhysisSceneIndexDrawerSettings();
        [Reactive] public bool ForceTrackerShowBodies { get; set; }
        [Reactive] public bool ForceTrackerShowJoints { get; set; }
        [Reactive] public bool ForceTrackerShowAxialFrictions { get; set; }
        public ReactiveCommand<Unit, Unit> ShowBodiesClick { get; private set; }
        public ReactiveCommand<Unit, Unit> ShowJointsClick { get; private set; }
        public ReactiveCommand<Unit, Unit> ShowAxialFrictionsClick { get; private set; }

        public SimulatorViewModel(EditorState editorState, Action buttonWasPressed)
        {
            this.editorState = editorState;
            this.cameraTrackerData = editorState.CameraTrackerData;

            this.RestartClick = ReactiveCommand.Create(() =>
            {
                buttonWasPressed(); //Weise der model-Variable was zu

                //Wenn die Simulation bei Restart noch läuft, dann pausiere sie zuerst
                if (this.isRunning)
                {
                    SetIsRunning(false);
                    model.Pause();
                }

                this.ShowReplayButton = false;
                this.State = SimulatorState.Restarted;

                if (model != null)
                    model.Restart();

                

                this.TimerTickCounter = "";
                this.LoadedReplayFile = lastLoadedFileName = null;
            });

            this.PlayPauseClick = ReactiveCommand.Create(() =>
            {
                SetIsRunning(!this.isRunning);

                buttonWasPressed();

                if (model != null)
                {
                    this.ShowReplayButton = this.LoadedReplayFile == null ? this.model.RecordDataAvailable() : true;

                    if (this.isRunning)
                    {
                        this.State = SimulatorState.Running;
                        model.Play();
                    }
                    else
                    {
                        this.State = SimulatorState.Paused;
                        model.Pause();
                    }
                }
            });

            this.SingleStepClick = ReactiveCommand.Create(() =>
            {
                buttonWasPressed();

                if (model != null)
                    model.DoSingleStep();
            });

            this.ReplayClick = ReactiveCommand.Create(() =>
            {
                SetIsRunning(true);
                this.State = SimulatorState.Replay;
                buttonWasPressed();

                if (model != null)
                {
                    if (string.IsNullOrEmpty(this.lastLoadedFileName) == false)
                    {
                        model.ReplayFromFile(lastLoadedFileName, () => SetIsRunning(false));
                    }
                    else if (this.model.RecordDataAvailable())
                    {
                        model.ReplayLastRun(() => SetIsRunning(false));
                    }
                }

            });
            this.SaveReplayClick = ReactiveCommand.Create(() =>
            {
                if (model != null && this.model.RecordDataAvailable())
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string recordData = model.GetRecordDataExportString();
                        File.WriteAllText(saveFileDialog.FileName, recordData);
                    }
                }

            });
            this.LoadReplayClick = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadedReplayFile = new FileInfo(openFileDialog.FileName).Name;
                    this.lastLoadedFileName = openFileDialog.FileName;
                    this.ShowReplayButton = true;

                    if (model != null)
                    {
                        model.ReplayFromFile(lastLoadedFileName, () => SetIsRunning(false));
                        model.Pause();
                    }

                    this.State = SimulatorState.Restarted;
                }
            });
            this.CopyPhysicSceneToClipboard = ReactiveCommand.Create(() =>
            {
                if (model != null)
                {
                    var exportObject = model.GetPhysicSceneExportData();
                    string exportString = JsonHelper.Helper.ToJson(exportObject);
                    System.Windows.Clipboard.SetText(exportString);
                }
            });
            

            this.ShowForceDataClick = ReactiveCommand.Create(() =>
            {
                this.ShowForceData = !this.ShowForceData;

            }, this.WhenAnyValue(x => x.State).Select(x => x == SimulatorState.Restarted)); //Zeige den ShowForceButton nur, wenn der State im Restaret ist

            this.WhenAnyValue(x => x.State).Subscribe(x =>
            {
                this.SimulatorIsActive = this.model != null;
                this.ShowForceDataButton = this.State == SimulatorState.Restarted;
                this.UseCameraTrackingCheckboxIsVisible = this.model != null && this.model.CameraTrackedItemAvailable();
            });


            //ContextMenü vom ForceTracker-Button
            this.ForceTrackerShowBodies = ForceTrackerDrawingSettings.ShowBodies;
            this.ForceTrackerShowJoints = ForceTrackerDrawingSettings.ShowJoints;
            this.ForceTrackerShowAxialFrictions = ForceTrackerDrawingSettings.ShowAxialFrictions;
            this.ShowBodiesClick = ReactiveCommand.Create(() =>
            {
                this.ForceTrackerDrawingSettings.ShowBodies = !this.ForceTrackerDrawingSettings.ShowBodies;
                this.ForceTrackerShowBodies = this.ForceTrackerDrawingSettings.ShowBodies;
            });
            this.ShowJointsClick = ReactiveCommand.Create(() =>
            {
                this.ForceTrackerDrawingSettings.ShowJoints = !this.ForceTrackerDrawingSettings.ShowJoints;
                this.ForceTrackerShowJoints = this.ForceTrackerDrawingSettings.ShowJoints;
            });
            this.ShowAxialFrictionsClick = ReactiveCommand.Create(() =>
            {
                this.ForceTrackerDrawingSettings.ShowAxialFrictions = !this.ForceTrackerDrawingSettings.ShowAxialFrictions;
                this.ForceTrackerShowAxialFrictions = this.ForceTrackerDrawingSettings.ShowAxialFrictions;
            });
        }

        internal void SetModel(SimulatorFunction model)
        {
            this.model = model;
            if (this.model != null)
            {
                this.model.TimerTickHandler += (s, d) =>
                {
                    //this.TimerTickCounter = ((this.model.GetTimerTickCounter() * this.timerIntercallInMilliseconds) / 1000).ToString("F1") + "s";
                    this.TimerTickCounter = this.model.GetTimerTickCounter().ToString();
                };
            }

            if (model == null)
                this.State = SimulatorState.Inactive;
            else
                this.State = SimulatorState.Restarted;
        }

        public void SetIsRunning(bool state)
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
    }
}
