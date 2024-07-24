using BridgeBuilderControl.Controls.Simulator.Model;
using GraphicPanels;
using GraphicPanelWpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;

namespace BridgeBuilderControl.Controls.Simulator
{
    internal class SimulatorViewModel : ReactiveObject, ITimerHandler, IGraphicPanelHandler
    {
        public class InputData
        {
            public Action GoBack; //Gehe zum BridgeEditor zurück
        }

        private GraphicPanel2D panel;
        private string dataFolder;
        private float timerIntervallInMilliseconds;
        private SimulateBridgeFunction model;

        public ReactiveCommand<Unit, Unit> EditClick { get; private set; }
        public ReactiveCommand<Unit, Unit> RunTrainClick { get; private set; }
        public ReactiveCommand<Unit, Unit> PauseClick { get; private set; }
        public ReactiveCommand<Unit, Unit> ResumeClick { get; private set; }
        public ReactiveCommand<Unit, Unit> StressClick { get; private set; }

        public ReactiveCommand<Unit, Unit> ZoomInMouseDown { get; private set; }
        public ReactiveCommand<Unit, Unit> ZoomInMouseUp { get; private set; }
        public ReactiveCommand<Unit, Unit> ZoomOutMouseDown { get; private set; }
        public ReactiveCommand<Unit, Unit> ZoomOutMouseUp { get; private set; }

        [Reactive] public bool TrainIsRunning { get; set; } = false;
        [Reactive] public bool SimulationIsRunning { get; set; } = true;
        [Reactive] public bool TrainHasPassedTheBridge { get; set; } = false;
        [Reactive] public bool TrainIsFallenIntoWater { get; set; } = false;

        [Reactive] public string ForceText { get; set; }
        [Reactive] public bool ShowForceText { get; set; } = false;

        private bool zoomInIsPressed = false;
        private bool zoomOutIsPressed = false;

        public SimulatorViewModel Init(InputData data)
        {
            this.EditClick = ReactiveCommand.Create(() =>
            {
                data.GoBack();
            });
            this.RunTrainClick = ReactiveCommand.Create(() =>
            {
                this.model.RunTrain();
                this.TrainIsRunning = this.model.TrainIsRunning();
            });
            this.PauseClick = ReactiveCommand.Create(() =>
            {
                this.SimulationIsRunning = this.model.SimulationIsRunning = false;
            });
            this.ResumeClick = ReactiveCommand.Create(() =>
            {
                this.SimulationIsRunning = this.model.SimulationIsRunning = true;
            });
            this.StressClick = ReactiveCommand.Create(() =>
            {
                this.model.ShowForces = !this.model.ShowForces;
            });

            this.ZoomInMouseDown = ReactiveCommand.Create(() =>
            {
                this.zoomInIsPressed = true;
            });

            this.ZoomInMouseUp = ReactiveCommand.Create(() =>
            {
                this.zoomInIsPressed = false;
            });

            this.ZoomOutMouseDown = ReactiveCommand.Create(() =>
            {
                this.zoomOutIsPressed = true;
            });

            this.ZoomOutMouseUp = ReactiveCommand.Create(() =>
            {
                this.zoomOutIsPressed = false;
            });

            return this;
        }

        

        public SimulatorViewModel(GraphicPanel2D panel, string dataFolder, float timerIntervallInMilliseconds)
        {
            this.panel = panel;
            this.dataFolder = dataFolder;
            this.timerIntervallInMilliseconds = timerIntervallInMilliseconds;
        }

        public void SimulateLevel(SimulatorInput input)
        {
            this.model = new SimulateBridgeFunction(this.panel, this.dataFolder, this.timerIntervallInMilliseconds, input, new Model.Converter.BridgeConverterSettings());
            this.TrainIsRunning = this.model.TrainIsRunning();
            this.SimulationIsRunning = this.model.SimulationIsRunning = true;
            this.ShowForceText = this.model.ShowPhysicModel;
            this.TrainHasPassedTheBridge = false;
            this.TrainIsFallenIntoWater = false;

            this.model.OnFirstBarIsBroken += Model_OnFirstBarIsBroken;
        }

        private void Model_OnFirstBarIsBroken(float force, float maxPull, float maxPush)
        {
            int frameCount = this.model.GetForceSampleCount();
            string reason = "";
            if (force < maxPull) reason = "Pull-Break";
            if (force > maxPush) reason = "Push-Break";
            this.ForceText = frameCount + " " + reason + " " + force;
        }

        #region ITimerHandler
        public void HandleTimerTick(float dt)
        {
            this.model.HandleTimerTick(dt, this.zoomInIsPressed, this.zoomOutIsPressed);
            if (this.TrainHasPassedTheBridge == false && this.TrainIsFallenIntoWater == false)
            {
                this.TrainHasPassedTheBridge = this.model.TrainHasPassedTheBridge();
                this.TrainIsFallenIntoWater = this.model.TrainIsInWater();
            }
            

            var force = this.model.GetMinMaxForce();
            int frameCount = this.model.GetForceSampleCount();
            if (this.model.FirstBarIsBroken == false)
            {
                this.ForceText = frameCount + " " + force.MinValue + " " + force.MaxValue;
            }
            
        }

        #endregion

        #region IGraphicPanelHandler
        public void HandleSizeChanged(int width, int height)
        {
            this.model.HandleSizeChanged(width, height);
        }
        public void HandleMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
        }
        public void HandleMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {

        }
        public void HandleMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            this.model.HandleMouseMove(e);
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
            if (e.Key == System.Windows.Input.Key.Escape || e.Key == System.Windows.Input.Key.E)
            {
                this.EditClick.Execute().Subscribe();
            }

            if (e.Key == System.Windows.Input.Key.T)
            {
                this.RunTrainClick.Execute().Subscribe();
            }

            if (e.Key == System.Windows.Input.Key.S)
            {
                this.StressClick.Execute().Subscribe();
            }

            if (e.Key == System.Windows.Input.Key.P)
            {
                if (this.model.SimulationIsRunning)
                    this.PauseClick.Execute().Subscribe();
                else
                    this.ResumeClick.Execute().Subscribe();
            }

            if (e.Key == System.Windows.Input.Key.A)
            {
                this.model.ShowPhysicModel = !this.model.ShowPhysicModel;
                this.ShowForceText = this.model.ShowPhysicModel;
            }

            if (e.Key == System.Windows.Input.Key.C)
            {
                this.model.CopyLevelToClipboard();
            }
        }
        public void HandleKeyUp(System.Windows.Input.KeyEventArgs e)
        {
        }


        #endregion
    }
}
