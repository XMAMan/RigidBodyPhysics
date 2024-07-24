using GameHelper;
using GameHelper.Simulation;
using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;
using PhysicSceneDrawing;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System;
using System.Drawing;

namespace ElmaControl.Controls.Game.Model
{
    internal class Flower : ITimerHandler, IRigidBodyDrawer
    {
        private SpriteImage sprite;
        private IPublicRigidCircle circle;
        private Bike bike;
        private Apples apples;

        public Vector2D Center { get => this.circle.Center.ToGrx(); }
        public float Radius { get => this.circle.Radius; }

        public event Action BikeIsTouching;


        public Flower(GameSimulator simulator, Bike bike, Apples apples, string dataFolder) 
        {
            this.circle = (IPublicRigidCircle)simulator.GetBodyByTagName("flower");
            this.sprite = new SpriteImage(dataFolder + "blume.png", 5, 1, 5, 1, false, 32, 32);
            this.sprite.Zoom = this.sprite.Width / this.Radius * 2;

            this.bike = bike;
            this.apples = apples;

            simulator.UseCustomDrawingForRigidBody(this.circle, this);
        }

        public void HandleTimerTick(float dt)
        {
            this.sprite.HandleTimerTick(dt);

            if (this.apples.Count == 0 && Intersect(this, this.bike))
            {
                this.BikeIsTouching?.Invoke();                
            }
        }

        private bool Intersect(Flower flower, Bike bike)
        {
            if ((flower.Center - bike.HeadCenter).Length() < (flower.Radius + bike.HeadRadius)) return true;
            if ((flower.Center - bike.Wheel1Center).Length() < (flower.Radius + bike.Wheel1Radius)) return true;
            if ((flower.Center - bike.Wheel2Center).Length() < (flower.Radius + bike.Wheel2Radius)) return true;

            return false;
        }

        public void Draw(GraphicPanel2D panel)
        {
            this.sprite.Draw(panel, this.circle.Center.ToGrx());
        }

        //Wird für die Minimap benötigt
        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            panel.DrawFillCircle(frontColor, this.circle.Center.ToGrx(), this.circle.Radius);
        }
    }
}
