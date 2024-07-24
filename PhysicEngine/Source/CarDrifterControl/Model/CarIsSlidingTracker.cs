using GraphicPanels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CarDrifterControl.Model
{
    //Speichert ber jeden TimerTick wo sehr die Räder seitlich rutschen
    //Außerdem zeichnet es die Bremsspur
    internal class CarIsSlidingTracker : ReactiveObject
    {
        class DataPoint
        {
            public Vec2D[] Position; //Position vom jeweiligen Rad
            public bool IsSlidingAnyWheel; //ist true, wenn alle 4 Räder schlittern
            public bool[] IsSliding; //Speichert für jedes Rad, ob es rutscht
        }

        private IPublicAxialFriction[] axialFrictions;
        private List<DataPoint> points = new List<DataPoint>();

        [Reactive] public bool IsSliding { get; set; } = false; //Rutschen gerade alle 4 Räder?

        public CarIsSlidingTracker(IPublicAxialFriction[] axialFrictions)
        {
            this.axialFrictions = axialFrictions;
        }

        public void StoreTrackingPoint()
        {
            var point = GetPoint();
            this.IsSliding = point.IsSlidingAnyWheel;
            this.points.Add(point);
        }

        private DataPoint GetPoint()
        {
            var isSliding = this.axialFrictions.Select(x => Math.Abs(x.Body.Velocity * x.ForceDirection.Normalize()) > 0.4f).ToArray();
            return new DataPoint()
            {
                Position = this.axialFrictions.Select(x => x.Anchor).Take(4).ToArray(),
                IsSliding = isSliding,
                IsSlidingAnyWheel = isSliding.Any(x => x),                
            };
        }

        public void Draw(GraphicPanel2D panel)
        {
            for (int i = 1; i < points.Count; i++)
            {
                var p1 = points[i];
                var p2 = points[i - 1];

                for (int j = 0; j < 4; j++)
                {
                    if (p1.IsSliding[j])
                    {
                        panel.DrawLine(Pens.Black, p2.Position[j].ToGrx(), p1.Position[j].ToGrx());
                    }                    
                }
            }
        }
    }
}
