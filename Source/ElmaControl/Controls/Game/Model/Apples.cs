using GameHelper;
using GameHelper.Simulation;
using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;
using PhysicSceneDrawing;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ElmaControl.Controls.Game.Model
{
    internal class Apple : ITimerHandler, IRigidBodyDrawer
    {
        private SpriteImage sprite;
        public IPublicRigidCircle Body { get; }

        public Vector2D Center { get => this.Body.Center.ToGrx(); }
        public float Radius { get => this.Body.Radius; }

        public Apple(GameSimulator simulator, IPublicRigidCircle body, string dataFolder)
        {
            this.Body = body;
            this.sprite = new SpriteImage(dataFolder + "apfel.png", 5, 1, 5, 1, false, 32, 32);

            this.sprite.Zoom = this.sprite.Width / this.Body.Radius * 3;

            simulator.UseCustomDrawingForRigidBody(body, this);
        }

        public void HandleTimerTick(float dt)
        {
            this.sprite.HandleTimerTick(dt);
        }

        public void Draw(GraphicPanel2D panel)
        {
            this.sprite.Draw(panel, this.Body.Center.ToGrx());
        }

        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            panel.DrawFillCircle(frontColor, this.Body.Center.ToGrx(), this.Body.Radius);
        }
    }

    //Speichert alle Äpfel, die im Level vorkommen
    internal class Apples : ITimerHandler
    {
        private List<Apple> apples;
        private GameSimulator simulator;
        private Sounds sounds;
        private Bike bike;

        public int Count { get => this.apples.Count; }

        public Apples(GameSimulator simulator, Sounds sounds, Bike bike, string dataFolder)
        {
            this.simulator = simulator;
            this.sounds = sounds;
            this.bike = bike;
            this.apples = simulator.GetBodiesByTagName("apple").Select(x => new Apple(simulator, (IPublicRigidCircle)x, dataFolder)).ToList();
        }

        public void HandleTimerTick(float dt)
        {
            //Wenn ich die PhysicScene den Kollisionstest machen lassen, dann prallt das Motorrad am Apfel ab
            //Deswegen mache ich das hier mit einer eigenen Funktion

            List<Apple> removeList = new List<Apple>();
            foreach (var apple in apples)
            {
                apple.HandleTimerTick(dt);

                if (Intersect(apple, this.bike))
                {
                    removeList.Add(apple);
                }
            }

            foreach (var apple in removeList)
            {
                this.apples.Remove(apple);
                this.simulator.RemoveRigidBody(apple.Body);
                this.sounds.PlayCollectAppel();
            }
        }

        private bool Intersect(Apple apple, Bike bike)
        {
            if ((apple.Center - bike.HeadCenter).Length() < (apple.Radius + bike.HeadRadius)) return true;
            if ((apple.Center - bike.Wheel1Center).Length() < (apple.Radius + bike.Wheel1Radius)) return true;
            if ((apple.Center - bike.Wheel2Center).Length() < (apple.Radius + bike.Wheel2Radius)) return true;

            return false;
        }

        public void Draw(GraphicPanel2D panel)
        {
            foreach (var apple in apples)
            {
                apple.Draw(panel);
            }
        }
    }
}
