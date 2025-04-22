using RigidBodyPhysics.MathHelper;
using System.Drawing;

namespace BridgeBuilderControl.Controls.Simulator.Model.Converter
{
    internal class AnchorPoint
    {
        public string Id { get; set; } //X_Y
        public Vec2D Position { get; set; }
        public Vec2D R1 { get; set; }  //Hebelarm im lokalen Bodyspace von B1.Center nach Anchor1-Punkt

        public AnchorPoint(Point point, Vec2D bodyCenter, float bodyAngleInDegree)
        {
            this.Id = point.X + "_" + point.Y;
            this.Position = new Vec2D(point.X, point.Y);
            var globalR1 = this.Position - bodyCenter;
            if (globalR1.Length() == 0)
                this.R1 = new Vec2D(0, 0);
            else
                this.R1 = Vec2D.GetV2FromAngle360(globalR1, -bodyAngleInDegree);
        }
    }
}
