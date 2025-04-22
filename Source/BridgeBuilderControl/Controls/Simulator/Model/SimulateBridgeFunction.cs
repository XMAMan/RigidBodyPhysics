using BridgeBuilderControl.Controls.Helper;
using BridgeBuilderControl.Controls.Simulator.Model.Converter;
using BridgeBuilderControl.Controls.Simulator.Model.Forcetracking;
using BridgeBuilderControl.Testing;
using GameHelper;
using GameHelper.Simulation;
using GraphicMinimal;
using GraphicPanels;
using LevelEditorGlobal;
using RigidBodyPhysics.RuntimeObjects.Joints;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BridgeBuilderControl.Controls.Simulator.Model
{
    internal class SimulateBridgeFunction : IBridgeSimulator
    {
        private GraphicPanel2D panel;
        private EditorCamera camera;
        private Vector2D screenMousePosition = new Vector2D(0, 0);
        private GameSimulator simulator;
        private PhysikLevelItemExportData trainExportData;
        private SimulatorInput input;
        private IPublicDistanceJoint[] bridgeDistanceJoints;
        private bool[] bridgeDistanceJointIsStreet;
        private float rightEdgeFromBridge = float.NaN;  //Rechte Seite von der Brücke. Bis hier hin muss der Zug es schaffen.
        private float waterHeight = float.NaN; //Y-Koordinate von der Wasseroberfläche
        private BridgeForceTracker forceTracker;
        private BridgeConverterSettings converterSettings;

        public bool SimulationIsRunning { get; set; } = true;
        public bool ShowPhysicModel { get; set; } = false;
        public bool FirstBarIsBroken { get; private set; } = false;

        public SimulateBridgeFunction(GraphicPanel2D panel, string dataFolder, float timerIntervallInMilliseconds, SimulatorInput input, BridgeConverterSettings converterSettings)
        {
            this.panel = panel;
            this.camera = input.Camera;
            this.input = input;
            this.converterSettings = converterSettings;

            this.simulator = EditorToSimulatorConverter.Convert(input, converterSettings, dataFolder, panel.Size, timerIntervallInMilliseconds, out trainExportData);

            this.bridgeDistanceJoints = this.simulator
                .GetJointsByTagName(EditorToSimulatorConverter.BridgeDistanceTagName)
                .Cast<IPublicDistanceJoint>()
                .ToArray();

            this.bridgeDistanceJointIsStreet = 
                this.bridgeDistanceJoints
                .Select(x => simulator
                    .GetTagDataFromJoint(x).Names.Contains(EditorToSimulatorConverter.BridgeDistanceStreetTagName))
                .ToArray();

            var lev = input.LevelExport;
            this.rightEdgeFromBridge = DrawingHelper.GetRightEdgeOfBridge(DrawingHelper.GetGroundPoints(lev.GroundPolygon, lev.GroundHeight, lev.XCount));
            this.waterHeight = DrawingHelper.GetWaterHeight(lev.WaterHeight, lev.GroundHeight);

            this.forceTracker = new BridgeForceTracker(this.bridgeDistanceJoints);

            this.simulator.JointWasDeletedHandler += (sender, joint) =>
            {
                if (this.FirstBarIsBroken == false && joint is IPublicDistanceJoint)
                {
                    var distanceJoint = (IPublicDistanceJoint)joint;
                    this.FirstBarIsBroken = distanceJoint.IsBroken;
                    this.OnFirstBarIsBroken?.Invoke(distanceJoint.AccumulatedImpulse, converterSettings.MaxPullForce, converterSettings.MaxPushForce);
                }
            };

            //RunTrain();
        }

        public bool ShowForces { get; set; } = true;

        public bool TrainIsRunning()
        {
            return simulator.GetBodiesByTagName("Train").Any();
        }

        public bool TrainHasPassedTheBridge()
        {
            if (TrainIsRunning() == false) return false;
            var lastTrainWheel = this.simulator.GetBodyByTagName("LastTrainWheel");
            return lastTrainWheel.Center.X > this.rightEdgeFromBridge;
        }

        public bool TrainIsInWater()
        {
            if (TrainIsRunning() == false) return false;
            var firstTrainWheel = this.simulator.GetBodyByTagName("FirstTrainWheel");
            return firstTrainWheel.Center.Y > this.waterHeight;
        }

        public void RunTrain()
        {
            if (TrainIsRunning()) return; //Zug ist bereits vorhanden. Macht nichts

            simulator.AddLevelItem(trainExportData);
            //simulator.PushBodysApart();
        }

        //Wenn ich das Level im LevelEditor mir ansehen will
        public void CopyLevelToClipboard()
        {
            this.simulator.CopyLevelToClipboard();
        }

        public void ZoomIn()
        {
            this.camera.ZoomIn();
        }

        public void ZoomOut()
        {
            this.camera.ZoomOut();
        }

        public void HandleTimerTick(float dt, bool zoomInIsPressed, bool zoomOutIsPressed)
        {
            if (SimulationIsRunning)
            {
                this.simulator.MoveOneStep(dt);
                this.forceTracker.AddSample();
                if (this.converterSettings.BridgeIsBreakable) RemoveBrokenDistanceJoints();
            }
            
            this.camera.MoveCameraWithMouse(this.screenMousePosition); //Bewege die Kamera, wenn die Maus am Bildschirmrand ist
            this.camera.SetZoom(zoomInIsPressed, zoomOutIsPressed);
            Refresh();
        }

        private void RemoveBrokenDistanceJoints()
        {
            var broken = this.forceTracker.GetAllDistanceJointsWhereTheMaxForceIsReachedForMoreThenNLastSteps(this.converterSettings.BreakAfterNSteps);
            foreach (var brokenJoint in broken)
            {
                this.simulator.RemoveJoint(brokenJoint);
            }
        }

        public BridgeForceTracker.MinMaxValue GetMinMaxForce()
        {
            return this.forceTracker.GetMinMax();
        }

        public int GetForceSampleCount()
        {
            return this.forceTracker.GetSampleCount();
        }

        public void HandleSizeChanged(int width, int height)
        {
            this.camera.UpdateScreenSize(width, height);
        }

        public void HandleMouseMove(MouseEventArgs e)
        {
            this.screenMousePosition = new Vector2D(e.X, e.Y);
        }

        private void Refresh()
        {
            panel.ClearScreen(Color.FromArgb(63, 69, 81));
            this.simulator.SetCameraMatrix(panel);

            var lev = this.input.LevelExport;

            //Z-Values: 0=Zug-Räder; 1=Zug-Gehäuse; 2 = Wasser; 3=Bar; 4=Street; 5 = GroundPolygon
            this.simulator.DrawPhysicItems(panel, false);

            panel.ZValue2D = 2;
            DrawingHelper.DrawWater(panel, lev.WaterHeight, lev.GroundHeight, lev.XCount, lev.YCount);
            
            for (int i=0;i<this.bridgeDistanceJoints.Length;i++)
            {
                var disJoint = this.bridgeDistanceJoints[i];
                if (disJoint.IsBroken) continue;
                bool isStreet = this.bridgeDistanceJointIsStreet[i];
                Color barColor = isStreet ? Color.White : Color.Gray;
                
                if (this.ShowForces)
                {
                    barColor = ForceColorCreator.GetForceColor(disJoint.AccumulatedImpulse, this.converterSettings.MaxPullForce, this.converterSettings.MaxPushForce).ToColor();
                }

                this.panel.ZValue2D = isStreet ? 4 : 3;
                DrawBar(panel, barColor, disJoint.Anchor1.ToGrx(), disJoint.Anchor2.ToGrx(), EditorToSimulatorConverter.BarWidth);
            }

            if (this.ShowPhysicModel)
            {
                this.simulator.DrawPhysicItemBorders(panel, Pens.Yellow);
                this.simulator.DrawDistanceJoints(panel, Pens.Blue);
                this.simulator.DrawCollisionPoints(panel);
            }
            

            panel.FlipBuffer();
        }

        private static void DrawBar(GraphicPanel2D panel, Color color, Vector2D p1, Vector2D p2, float width)
        {
            float length = (p2 - p1).Length();
            var direction = (p2 - p1).Normalize();

            var center = (p1 + p2) / 2;
            float angleInDegree = Vector2D.Angle360(new Vector2D(1, 0), direction);

            panel.DrawFillRectangle(color, center.X, center.Y, length, width, angleInDegree);
        }

        #region IBridgeSimulator
        
        public event FirstBarIsBroken OnFirstBarIsBroken;
        public void DoTimeStep(float dt)
        {
            this.simulator.MoveOneStep(dt);
            this.forceTracker.AddSample();
            if (this.converterSettings.BridgeIsBreakable) RemoveBrokenDistanceJoints();
        }

        public float[] GetPullForcesForEachTimeStep()
        {
            return this.forceTracker.GetMinMaxValuesForEachTimeStep().Select(x => x.MinValue).ToArray();
        }

        public float[] GetPushForcesForEachTimeStep()
        {
            return this.forceTracker.GetMinMaxValuesForEachTimeStep().Select(x => x.MaxValue).ToArray();
        }

        public float GetMaxAllowedPullForce()
        {
            return this.bridgeDistanceJoints[0].MinForceToBreak;
        }

        public float GetMaxAllowedPushForce()
        {
            return this.bridgeDistanceJoints[0].MaxForceToBreak;
        }

        public float GetMaxPullForce()
        {
            return this.GetMinMaxForce().MinValue;
        }

        public float GetMaxPushForce()
        {
            return this.GetMinMaxForce().MaxValue;
        }

        public bool SomeBarsAreBroken()
        {
            return this.bridgeDistanceJoints.Any(x => x.IsBroken);
        }
        #endregion
    }
}
