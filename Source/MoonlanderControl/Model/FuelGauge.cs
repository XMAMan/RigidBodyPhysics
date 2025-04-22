using GraphicPanels;
using System;
using System.Drawing;
using GameHelper.Simulation.RigidBodyTagging;
using GameHelper;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace MoonlanderControl.Model
{
    internal class FuelGauge
    {
        private const float maxFuelAmount = 5000; //So viele Millisekundne reicht der Treibstoff insgesamt

        private float fuelAmount = maxFuelAmount; //Aktuelle Treibstoffmenge
        private IPublicThruster mainThruster;
        private ColorInterpolator colorInterpolator = new ColorInterpolator(new Color[] { Color.Green, Color.Red, });

        public bool IsFuelAvailable()
        {
            return this.fuelAmount > 0;
        }

        public event Action FuelBecomesEmptyHandler;

        public FuelGauge(ITagDataProvider tagProvider)
        {
            this.mainThruster = tagProvider.GetThrusterByTagName("mainThruster");
        }

        public void TimerTickHandler(float dt)
        {
            if (this.mainThruster.IsEnabled)
            {
                this.fuelAmount -= dt;
                if (this.fuelAmount < 0)
                {
                    this.fuelAmount = 0;
                    this.mainThruster.IsEnabled = false;
                    this.FuelBecomesEmptyHandler?.Invoke();
                }
            }
        }

        public void Draw(GraphicPanel2D panel)
        {
            float f = this.fuelAmount / maxFuelAmount;

            int width = 200;
            int height = 20;

            int border = 15; //Abstand zum rechten oberen Rand
            int b = 2; //Abstand vom FillRectangle zum Border-Rectangle

            panel.DrawRectangle(new Pen(Color.Yellow, 2), panel.Width - border - width, border, width, height);

            int maxLength = width - b * 2;
            float length = f * maxLength;
            panel.DrawFillRectangle(this.colorInterpolator.GetColor(f), panel.Width - border - width + b, border + b, length, height - b * 2);
        }
    }
}
