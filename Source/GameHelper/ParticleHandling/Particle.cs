using GraphicPanels;
using PhysicGlobal;

namespace GameHelper.ParticleHandling
{
    public class Particle
    {
        private ColorInterpolator colorInterpolator;
        private Vec2D position;
        private Vec2D velocity;
        private float lifeTime; //Geht von 0 bis 1
        private float agingRate;
        private float size;

        public Particle(ColorInterpolator colorInterpolator, Vec2D position, Vec2D velocity, float agingRate, float size)
        {
            this.colorInterpolator = colorInterpolator;
            this.position = position;
            this.velocity = velocity;
            this.lifeTime = 0;
            this.agingRate = agingRate;
            this.size = size;
        }

        public void MoveOnStep(float dt)
        {
            this.position += velocity * dt;
            this.lifeTime += this.agingRate * dt;
        }

        //Ist die Lebenszeit vom Partikel zu Ende?
        public bool IsFinished()
        {
            return this.lifeTime > 1;
        }

        public void Draw(GraphicPanel2D panel)
        {
            panel.DrawFillRectangle(colorInterpolator.GetColor(this.lifeTime), this.position.X - size, this.position.Y - size, size * 2, size * 2);
        }
    }
}
