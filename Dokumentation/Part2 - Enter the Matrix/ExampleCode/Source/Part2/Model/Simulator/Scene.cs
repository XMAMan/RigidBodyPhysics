using GraphicPanels;
using Part2.Model.ShapeExporter;
using Part2.Model.Simulator.SimulatorShape;
using Part2.ViewModel;
using PhysicEngine;
using PhysicEngine.CollisionDetection;
using PhysicEngine.CollisionDetection.NearPhase;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Part2.Model.Simulator
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
        public bool HasGravity
        {
            get => this.scene.HasGravity;
            set => this.scene.HasGravity = value;
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
    }
}
