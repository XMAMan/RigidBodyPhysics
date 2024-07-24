using KeyboardRecordAndPlay;
using LevelEditorControl.LevelItems.GroupedItems;
using LevelEditorControl.LevelItems;
using LevelEditorGlobal;
using LevelToSimulatorConverter;
using System.IO;
using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using Simulator.ForceTracking;
using Simulator;
using RigidBodyPhysics.ExportData;

namespace LevelEditorControl.EditorFunctions
{
    internal class SimulatorFunction : DummyFunction, IEditorFunction
    {
        enum RunningState
        {
            Record, //Simulation läuft und Tastendrücke werden aufgzeichnet
            Replay, //Replay wird abgespielt
        }

        private EditorState state;
        private ILeveleditorUsedSimulator simulator;
        private RunningState runningState = RunningState.Record;
        private KeyboardRecorder keyboardRecorder;
        private KeyBoardPlayer keyboardPlayer = null;

        //Fürs ForceTracking
        private PhysisSceneIndexDrawerSettings forceTrackerDrawingSettings;
        private PhysisSceneIndexDrawer indexDrawer; 

        public bool IsRunning { get; private set; } = false;
        public event EventHandler TimerTickHandler;//Zur Aktualisierung des TimerTickCounters in der View und im ForceTracker        
        public Action<ILeveleditorUsedSimulator> SimulatorChangedHandler { get; set; } = null; //Damit wird dem Forcetracker gesagt, dass ein neuer Simulationsrun läuft

        public override FunctionType Type { get; } = FunctionType.Simulator;

        public override IEditorFunction Init(EditorState state)
        {
            throw new NotImplementedException();
        }

        public IEditorFunction Init(EditorState state, PhysisSceneIndexDrawerSettings forceTrackerDrawingSettings)
        {
            this.state = state;
            this.forceTrackerDrawingSettings = forceTrackerDrawingSettings;

            return this;
        }

        public int GetTimerTickCounter()
        {
            if (this.keyboardPlayer != null)
                return this.keyboardPlayer.TimerTickCounter;

            if (this.keyboardRecorder != null)
                return this.keyboardRecorder.TimerTickCounter;

            return 0;
        }

        public SimulatorInputData GetSimulatorExportData()
        {
            return SimulatorExporter.Convert(GetSimulatorInputData());
        }

        public void Restart()
        {
            this.simulator = state.CreateSimulator(SimulatorExporter.Convert(GetSimulatorInputData()), this.state.Panel.Size, state.Camera, state.TimerIntervallInMilliseconds);

            this.keyboardRecorder = new KeyboardRecorder();
            this.keyboardPlayer = null;
            this.runningState = RunningState.Record;
            this.indexDrawer = this.simulator.CreatePhysisSceneIndexDrawer(this.forceTrackerDrawingSettings);

            this.SimulatorChangedHandler?.Invoke(this.simulator);
        }

        private EditorDataForSimulation GetSimulatorInputData()
        {
            var items = state.LevelItems.Where(x => x is IPhysicMergerItem).Cast<IPhysicMergerItem>().ToList();

            //PhysicItems aus dem GroupedItemsLevelItem
            items.AddRange(state.LevelItems
                .Where(x => x is IPhysicSceneContainer)
                .Cast<IPhysicSceneContainer>()
                .SelectMany(x => x.GetPhysicMergerItems())

                );

            var backgroundItems = state.LevelItems
                    .Where(x => x is IBackgroundItemProvider)
                    .Cast<IBackgroundItemProvider>()
                    .SelectMany(x => x.GetBackgroundItems())
                    .ToList();

            backgroundItems.AddRange(state.LevelItems
                .Where(x => x is IBackgroundItem)
                .Cast<IBackgroundItem>());

            return new EditorDataForSimulation()
            {
                Items = items,
                KeyboardMappings = state.KeyboradMappings.ToArray(),
                CollisionMatrix = state.CollisionMatrixViewModel.CollideMatrix,
                TimerIntercallInMilliseconds = state.TimerIntervallInMilliseconds,
                BackgroundImage = state.PolygonImages.Background,
                ForegroundImage = state.PolygonImages.ForegroundImage,
                HasGravity = state.HasGravity,
                Gravity = state.Gravity,
                IterationCount = state.SimulatorIterationCount,
                CameraTrackedLevelItemId = state.CameraTrackedItem != null ? state.CameraTrackedItem.Id : -1,
                Panel = state.Panel,
                Camera = state.Camera,
                CameraTrackerData = state.CameraTrackerData,
                BackgroundItems = backgroundItems.Select(x => x.GetSimulatorExportData()).ToArray(),
                TagData = state.GetAllTagsForSimulator()
            };
        }

        public void Play()
        {
            if (this.simulator == null) this.Restart();
            this.IsRunning = true;
        }

        public void Pause()
        {
            this.IsRunning = false;
        }

        public void DoSingleStep()
        {
            if (this.simulator == null) this.Restart();

            if (this.runningState == RunningState.Replay)
            {
                this.keyboardPlayer.HandleTimerTick(state.TimerIntervallInMilliseconds);
            }
            else
            {
                this.keyboardRecorder.HandleTimerTick(state.TimerIntervallInMilliseconds);
            }

            this.simulator.MoveOneStep(state.TimerIntervallInMilliseconds);

            this.TimerTickHandler?.Invoke(this, EventArgs.Empty);
        }

        public bool RecordDataAvailable()
        {
            return this.keyboardRecorder != null;
        }

        public string GetRecordDataExportString()
        {
            var data = this.keyboardRecorder.GetRecordedData();

            return JsonHelper.Helper.ToJson(data);
        }

        public void ReplayLastRun(Action isFinish)
        {
            var lastRunRecorder = this.keyboardRecorder;
            this.Restart();
            this.keyboardPlayer = new KeyBoardPlayer(new KeyBoardPlayerConstructorData()
            {
                RecordData = lastRunRecorder.GetRecordedData(),
                KeyDownAction = (key) => this.simulator.HandleKeyDown(key),
                KeyUpAction = (key) => this.simulator.HandleKeyUp(key),
                IsFinish = () => { this.IsRunning = false; isFinish(); }
            });
            this.keyboardRecorder = lastRunRecorder;
            this.IsRunning = true;
            this.runningState = RunningState.Replay;
        }

        public void ReplayFromFile(string fileName, Action isFinish)
        {
            this.Restart();

            var recordData = JsonHelper.Helper.CreateFromJson<KeyBoardRecordData>(File.ReadAllText(fileName));

            this.keyboardPlayer = new KeyBoardPlayer(new KeyBoardPlayerConstructorData()
            {
                RecordData = recordData,
                KeyDownAction = (key) => this.simulator.HandleKeyDown(key),
                KeyUpAction = (key) => this.simulator.HandleKeyUp(key),
                IsFinish = () => { this.IsRunning = false; isFinish(); }
            });

            this.IsRunning = true;
            this.runningState = RunningState.Replay;
        }

        public PhysicSceneExportData GetPhysicSceneExportData()
        {
            return this.simulator.GetPhysicSceneExportData();
        }

        public override void HandleTimerTick(float dt)
        {
            if (this.IsRunning)
            {
                if (this.runningState == RunningState.Replay)
                {
                    this.keyboardPlayer.HandleTimerTick(dt);
                }
                else
                {
                    this.keyboardRecorder.HandleTimerTick(dt);
                }

                this.simulator.MoveOneStep(dt);

                this.TimerTickHandler?.Invoke(this, EventArgs.Empty);
            }
            

            Draw();
        }

        private void Draw()
        {
            var panel = state.Panel;

            if (this.state.ShowForceData == false)
            {
                this.simulator.Draw(panel);
            }else
            {                
                panel.ClearScreen(Color.White);
                panel.MultTransformationMatrix(state.Camera.GetPointToSceenMatrix());

                //Gibt zu jeden Körper und Gelenk die Indizes aus. Wird für den ForceTracker benötigt
                this.simulator.DrawPhysicItemBorders(panel, Pens.Black);
                this.indexDrawer.Draw(panel);

                this.simulator.DrawSmallWindow(panel);                
            }

            panel.FlipBuffer();
        }

        public override void HandleSizeChanged(int width, int height)
        {
            if (this.simulator != null)
            {
                this.simulator.PanelSizeChangedHandler(width, height);
            }
        }

        public override void HandleMouseDown(MouseEventArgs e)
        {
            if (this.simulator != null)
            {
                this.simulator.HandleMouseDown(e);
            }
        }

        public override void HandleMouseUp(MouseEventArgs e)
        {
            if (this.simulator != null)
            {
                this.simulator.HandleMouseUp(e);
            }
        }

        public override void HandleMouseMove(MouseEventArgs e)
        {
            if (this.simulator != null)
            {
                this.simulator.HandleMouseMove(e);
            }
        }

        public override void HandleMouseWheel(MouseEventArgs e)
        {
            if (this.simulator != null)
            {
                this.simulator.HandleMouseWheel(e);
            }
        }

        public override void HandleKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (this.simulator != null && this.IsRunning && this.runningState == RunningState.Record)
            {
                this.keyboardRecorder.AddKeyDownEvent(e.Key);
                this.simulator.HandleKeyDown(e.Key);
            }

            e.Handled = true;
        }

        public override void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (this.simulator != null && this.IsRunning && this.runningState == RunningState.Record)
            {
                this.keyboardRecorder.AddKeyUpEvent(e.Key);
                this.simulator.HandleKeyUp(e.Key);
            }
            e.Handled = true;
        }

        public bool CameraTrackedItemAvailable()
        {
            return state.CameraTrackedItem != null;
        }
    }
}
