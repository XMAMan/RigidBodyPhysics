using GraphicMinimal;
using GraphicPanels;
using System.Drawing;
using System;
using System.Collections.Generic;
using GameHelper.Simulation;
using GameHelper.Simulation.RigidBodyTagging;
using GameHelper;
using GameHelper.ParticleHandling;
using RigidBodyPhysics.RuntimeObjects.Thruster;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace MoonlanderControl.Model
{
    //Simuliert die Bewegung von Feuerpartikeln von der Hauptschubdüse des Raumschiffes
    class MainThruster
    {
        private static ColorInterpolator colorInterpolator = new ColorInterpolator(new Color[] { Color.White, Color.BlueViolet, Color.Yellow, Color.Red });


        private AnchorPoint anchorPoint1, anchorPoint2;
        private Random rand = new Random(0);
        private ParticleHandler particleHandler = new ParticleHandler();
        private IPublicThruster mainThruster;
        private IPublicRigidBody ship;
        private bool shipIsRemovedFromSimulation = false;

        private Sounds sounds;

        public MainThruster(GameSimulator simulator, Sounds sounds) 
        {
            this.sounds = sounds;

            this.anchorPoint1 = new AnchorPoint(simulator, "ship", 0);
            this.anchorPoint2 = new AnchorPoint(simulator, "ship", 1);
            this.mainThruster = simulator.GetThrusterByTagName("mainThruster");
            this.ship = simulator.GetBodyByTagName("ship");

            simulator.BodyWasDeletedHandler += Simulator_BodyWasDeletedHandler;
            this.mainThruster.IsEnabledChanged += MainThruster_IsEnabledChanged;
        }

        private void MainThruster_IsEnabledChanged(bool isEnabled)
        {
            if (isEnabled)
                this.sounds.StartMainThrusterSound();
            else
                this.sounds.StopMainThrusterSound();
        }

        private void Simulator_BodyWasDeletedHandler(RigidBodyPhysics.PhysicScene sender, IPublicRigidBody body)
        {
            if (body == this.ship)
            {
                this.shipIsRemovedFromSimulation = true;
            }
        }

        public bool IsEnabled { get => this.mainThruster.IsEnabled; }

        public void Draw(GraphicPanel2D panel)
        {
            //var p1 = GetAnchor1();
            //var p2 = GetAnchor2();
            //panel.DrawLine(new Pen(Color.Blue, 3), p1, p2);

            this.particleHandler.DrawParticles(panel);
        }

        public void MoveOnStep(float dt)
        {
            if (this.mainThruster.IsEnabled && this.shipIsRemovedFromSimulation == false)
            {
                this.particleHandler.AddParticles(CreateNewParticles());
            }            

            this.particleHandler.HandleTimerTick(dt);                
        }

        private List<Particle> CreateNewParticles()
        {
            List<Particle> particles = new List<Particle>();

            var p1 = this.anchorPoint1.GetPosition();
            var p2 = this.anchorPoint2.GetPosition();

            var direction = (p2 - p1).Spin90().Normalize();

            int count = this.rand.Next(10);

            for (int i = 0; i < count; i++)
            {
                float fPos = (float)rand.NextDouble();
                Vector2D position = (1 - fPos) * p1 + fPos * p2;

                float fVel = (float)rand.NextDouble();
                fVel = fVel / 8 + 0.25f;
                Vector2D velocity = direction * fVel;

                float agingRate = 0.001f;

                var particle = new Particle(colorInterpolator, position, velocity, agingRate, 1);
                particles.Add(particle);
            }
            return particles;
        }
    }
}
