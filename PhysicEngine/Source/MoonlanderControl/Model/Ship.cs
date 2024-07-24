using GraphicMinimal;
using System;
using System.Collections.Generic;
using System.Linq;
using GameHelper.Simulation.RigidBodyTagging;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace MoonlanderControl.Model
{
    class Ship
    {
        private IPublicJoint weld1, weld2;
        private AnchorPoint shipPoint1, shipPoint2;

        private Queue<bool> isStandingQueue = new Queue<bool>(); //Merkt sich von den letzten 5 TimerTicks ob das Schiff unversehrt auf der Plattform steht

        private bool isBroken = false;
        public bool IsBroken 
        {
            get => this.isBroken;
            private set
            {
                if (this.isBroken != value)
                {
                    this.isBroken = value;
                    this.IsBrokenChanged?.Invoke(this.isBroken);
                }                
            }
        }

        public event Action<bool> IsBrokenChanged;

        public bool IsStandingOnLandingArea { get; private set; } = false;

        public Ship(ITagDataProvider tagProvider)
        {
            this.weld1 = tagProvider.GetJointByTagName("weld1");
            this.weld2 = tagProvider.GetJointByTagName("weld2");

            this.shipPoint1 = new AnchorPoint(tagProvider, "leg1", 0);
            this.shipPoint2 = new AnchorPoint(tagProvider, "leg2", 0);
        }

        public Vector2D GetLeg1()
        {
            return this.shipPoint1.GetPosition();
        }

        public Vector2D GetLeg2()
        {
            return this.shipPoint2.GetPosition();
        }

        public float GetDistanceToLandingArea(LandingArea landingArea)
        {
            float d1 = (GetLeg1() - landingArea.Center).Length();
            float d2 = (GetLeg2() - landingArea.Center).Length();

            return Math.Min(d1, d2);
        }

        public bool IsShipInMinMaxRangeFromLandingArea(LandingArea landingArea)
        {
            var p1 = GetLeg1();
            var p2 = GetLeg2();

            bool b1 = p1.X >= landingArea.MinX && p1.X <= landingArea.MaxX;
            bool b2 = p2.X >= landingArea.MinX && p2.X <= landingArea.MaxX;

            return b1 && b2;
        }

        public void TimerTickHandler(Ground ground)
        {
            this.IsBroken = this.weld1.IsBroken || this.weld2.IsBroken;

            int d1 = (int)ground.GetDistanceToGround(GetLeg1());
            int d2 = (int)ground.GetDistanceToGround(GetLeg2());
            bool newValue = this.IsBroken == false && d1 == 0 && d2 == 0 && IsShipInMinMaxRangeFromLandingArea(ground.LandingArea);

            if (this.isStandingQueue.Count > 5)
            {
                this.isStandingQueue.Dequeue(); //Entferne den letzten Wert
            }
            this.isStandingQueue.Enqueue(newValue);

            this.IsStandingOnLandingArea = this.isStandingQueue.All(x => x); //Nur wenn die letzten 5 TimerTicks alle Werte true waren, steht das Raumschiff wirklich
        }
    }
}
