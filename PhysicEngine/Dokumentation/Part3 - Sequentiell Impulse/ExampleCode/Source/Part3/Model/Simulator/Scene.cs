using GraphicPanels;
using Part3.Model.ShapeExporter;
using Part3.Model.Simulator.SimulatorShape;
using Part3.ViewModel;
using PhysicEngine;
using PhysicEngine.RigidBody;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Part3.Model.Simulator
{
    //Enthält Menge von Rechtecken und Kreisen
    class Scene : IShapeDataContainer
    {
        private PhysicScene scene = new PhysicScene();
        private List<ISimulatorShape> shapes = new List<ISimulatorShape>();

        public bool ShowCollisionPoints { get; set; } = false;
        public bool DoPositionalCorrection
        {
            get => this.scene.DoPositionalCorrection;
            set => this.scene.DoPositionalCorrection = value;
        }
        public bool DoWarmStart
        {
            get => this.scene.DoWarmStart;
            set => this.scene.DoWarmStart = value;
        }
        public bool HasGravity
        {
            get => this.scene.HasGravity;
            set => this.scene.HasGravity = value;
        }

        public PhysicScene.SolverType SolverType
        {
            get => this.scene.Solver;
            set => this.scene.Solver = value;
        }
                

        //Aktualisiert die Position der Objekte
        //dt = So viele Millisekunden sind vergangen
        public void TimeStep(float dt)
        {
            this.scene.TimeStep(dt);        
        }

        public void Draw(GraphicPanel2D panel)
        {
            panel.ClearScreen(Color.White);
            //panel.DrawLine(Pens.Black, new Vector2D(0, 0), new Vector2D(panel.Width, panel.Height));
            
            foreach (ISimulatorShape shape in this.shapes) { shape.Draw(panel); }            

            if (this.ShowCollisionPoints)
            {
                foreach (var c in this.scene.GetCollisions())
                {
                    panel.DrawFillCircle(Color.Green, c.Start, 3);
                    panel.DrawFillCircle(Color.Blue, c.End, 3);
                    panel.DrawLine(Pens.Red, c.Start, c.Start + c.Normal * 20);
                }
            }
            

            panel.FlipBuffer();
        }

        public string GetShapeData()
        {
            return ExportHelper.ToJson(this.shapes);
        }

        public void LoadShapeData(string json)
        {            
            this.shapes = ExportHelper.JsonToSimulatorShape(json);
            this.scene.Reload(this.shapes.Select(x => x.PhysicModel).ToList());            
        }

        public void PushBodysApart()
        {
            this.scene.PushBodysApart();
        }

        public string GetStateFromAllBodies()
        {
            return string.Join(";", this.shapes.Select(x => BodyToString(x.PhysicModel)));
        }

        private static string BodyToString(IRigidBody body)
        {
            return "[" +
                FloatToString(body.Center.X) + " " + FloatToString(body.Center.Y) + FloatToString(body.Angle) +
                FloatToString(body.Velocity.X) + " " + FloatToString(body.Velocity.Y) + FloatToString(body.AngularVelocity) + "]";
        }

        private static string FloatToString(float x)
        {
            return x.ToString("G9");
            //return String.Format("{0:+0.00000;-0.00000; 0.00000}", x);
        }
    }
}
