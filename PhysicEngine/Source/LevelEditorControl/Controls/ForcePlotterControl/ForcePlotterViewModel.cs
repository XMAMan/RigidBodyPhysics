using DynamicData;
using LevelEditorControl.EditorFunctions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using Simulator.ForceTracking;

namespace LevelEditorControl.Controls.ForcePlotterControl
{
    internal class ForcePlotterViewModel : ReactiveObject
    {
        private EditorState editorState;
        private SimulatorFunction simulatorFunction = null;
        private ForceTracker forceTracker; //Zeichnet die Kräfte auf, die auf die Körper und Gelenke wirken

        public ObservableCollection<float> ForceSamples { get; private set; } = new ObservableCollection<float>();

        [Reactive] public int MaxTime { get; set; } //Der Forceplotter-Graph ist 10 Sekunden lang
        [Reactive] public float MinValue { get; set; }
        [Reactive] public float MaxValue { get; set; }
        [Reactive] public float SelectedValue { get; set; }


        public ObservableCollection<string> ForceNames { get; private set; } = new ObservableCollection<string>();
        [Reactive] public string SelectedForceName { get; set; }

        public ForcePlotterViewModel(EditorState editorState)
        {
            this.editorState = editorState;
        }

        internal void SetModel(SimulatorFunction model)
        {
            this.simulatorFunction = model;
            this.forceTracker = null;
            ForceSamples.Clear();
            if (model != null)
            {
                //Es beginnt ein neuer Simulationsrun -> Ermittle welche ForceNames es gibt
                model.SimulatorChangedHandler = (sim) =>
                {
                    int lastSelectedForceIndex = this.ForceNames.IndexOf(this.SelectedForceName);

                    this.forceTracker = sim.CreateForceTracker();

                    ForceSamples.Clear();
                    this.ForceNames.Clear();
                    this.ForceNames.AddRange(this.forceTracker.GetTrackerNames());

                    if (lastSelectedForceIndex != -1 && lastSelectedForceIndex < ForceNames.Count)
                        this.SelectedForceName = this.ForceNames[lastSelectedForceIndex];
                    else
                        if (this.ForceNames.Count > 0)
                        this.SelectedForceName = this.ForceNames[0];

                    this.MaxTime = (int)this.editorState.TimerIntervallInMilliseconds * 10;
                };

                //TimerTick -> Füge ForceSample hinzu
                model.TimerTickHandler += (s, d) =>
                {
                    if (this.forceTracker == null || this.editorState.ShowForceData == false) return;

                    this.forceTracker.AddDataRow();

                    int time = this.simulatorFunction.GetTimerTickCounter();

                    if (time > this.MaxTime) this.MaxTime = time;

                    int selectedForceIndex = this.ForceNames.IndexOf(this.SelectedForceName);
                    if (selectedForceIndex != -1 && time > this.ForceSamples.Count) //Gibt es ein neues Sample, was noch nicht in ForceSamples gespeichert ist?
                    {
                        float sample = this.forceTracker.GetSingleSample(selectedForceIndex, time - 1);

                        this.ForceSamples.Add(sample);
                    }
                };
            }

            //Wenn ein anderer ForceTracker aktiviert wird, dann lade alle Samples in den Plotter
            this.WhenAnyValue(x => x.SelectedForceName).Subscribe(x =>
            {
                if (this.forceTracker == null) return;

                int selectedForceIndex = this.ForceNames.IndexOf(this.SelectedForceName);
                if (selectedForceIndex != -1)
                {
                    float[] samples = this.forceTracker.GetAllSamplesFromASingleTracker(selectedForceIndex);

                    this.ForceSamples.Clear();
                    this.ForceSamples.AddRange(samples);
                }
            });
        }
    }
}
