using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace GameHelper.ParticleHandling
{
    //Bewegt/Zeichnet eine Menge von Partikeln
    public class ParticleHandler : ITimerHandler
    {
        private List<Particle> particles = new List<Particle>();

        public void AddParticles(List<Particle> particles)
        {
            this.particles.AddRange(particles);
        }

        public void HandleTimerTick(float dt)
        {
            if (this.particles.Any())
            {
                MoveParticles(dt);
            }
        }

        public void DrawParticles(GraphicPanel2D panel)
        {
            foreach (var particle in this.particles)
            {
                particle.Draw(panel);
            }
        }

        private void MoveParticles(float dt)
        {
            List<Particle> finishedParticles = new List<Particle>();
            foreach (var particle in this.particles)
            {
                particle.MoveOnStep(dt);
                if (particle.IsFinished())
                    finishedParticles.Add(particle);
            }

            foreach (var particle in finishedParticles)
            {
                this.particles.Remove(particle);
            }
        }

        public static List<Particle> CreateParticles(IPublicRigidBody body, int count, float particleSize, Color color1, Color color2, Random rand)
        {
            if (body is IPublicRigidRectangle)
                return CreateParticles((IPublicRigidRectangle)body, count, particleSize, color1, color2, rand);

            if (body is IPublicRigidPolygon)
                return CreateParticles((IPublicRigidPolygon)body, count, particleSize, color1, color2, rand);

            throw new NotImplementedException();
        }

        //Erzeugt innerhalb eines Rechtecks count Partikel
        public static List<Particle> CreateParticles(IPublicRigidRectangle r, int count, float particleSize, Color color1, Color color2, Random rand)
        {
            List<Particle> particles = new List<Particle>();

            Vector2D p0 = r.Vertex[0].ToGrx();
            Vector2D v1 = r.Vertex[1].ToGrx() - r.Vertex[0].ToGrx();
            Vector2D v2 = r.Vertex[3].ToGrx() - r.Vertex[0].ToGrx();
            Vector2D center = r.Center.ToGrx();

            var colorInterpolator = new ColorInterpolator(new Color[] { color1, color2 });

            for (int i = 0; i < count; i++)
            {
                Vector2D position = p0 + v1 * (float)rand.NextDouble() + v2 * (float)rand.NextDouble();
                Vector2D direction = (position - center).Normalize();

                float fVel = (float)rand.NextDouble();
                fVel = fVel / 8 + 0.25f;
                Vector2D velocity = direction * fVel;

                float agingRate = 0.001f;

                var particle = new Particle(colorInterpolator, position, velocity, agingRate, particleSize);
                particles.Add(particle);
            }

            return particles;
        }

        //Erzeugt innerhalb eines Polygons count Partikel
        public static List<Particle> CreateParticles(IPublicRigidPolygon poly, int count, float particleSize, Color color1, Color color2, Random rand)
        {
            List<Particle> particles = new List<Particle>();

            var center = poly.Center.ToGrx();
            float radius = (poly.Vertex[0] - poly.Center).Length();

            var colorInterpolator = new ColorInterpolator(new Color[] { color1, color2 });

            for (int i = 0; i < count; i++)
            {
                double rad = 2 * Math.PI * rand.NextDouble();
                Vector2D position = center + new Vector2D((float)Math.Cos(rad), (float)Math.Sin(rad)) * radius * (float)rand.NextDouble();
                Vector2D direction = (position - center).Normalize();


                float fVel = (float)rand.NextDouble();
                fVel = fVel / 8 + 0.25f;
                Vector2D velocity = direction * fVel;

                float agingRate = 0.001f;

                var particle = new Particle(colorInterpolator, position, velocity, agingRate, particleSize);
                particles.Add(particle);
            }

            return particles;
        }
    }
}
