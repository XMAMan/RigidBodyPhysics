using GameHelper;
using GraphicPanels;
using System;
using System.Drawing;
using GameHelper.Simulation;
using DynamicObjCreation.RigidBodyDestroying;
using RigidBodyPhysics.MathHelper;
using GameHelper.ParticleHandling;
using RigidBodyPhysics.RuntimeObjects.Thruster;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace MoonlanderControl.Model
{
    //Läßt das Rechteck vom Raumschiff nach N Sekunden explodieren
    class ShipExploder
    {
        private TickCounterStopwatch waitForExplosionTimer;
        private GameSimulator simulator;
        private IPublicRigidBody ship;
        private IPublicThruster mainThruster;
        private ParticleHandler particleHandler = new ParticleHandler();
        private Random rand;

        private Sounds sounds;

        public ShipExploder(GameSimulator simulator, Sounds sounds, Random rand) 
        {
            this.simulator = simulator;
            this.ship = simulator.GetBodyByTagName("ship");
            this.mainThruster = simulator.GetThrusterByTagName("mainThruster");
            this.sounds = sounds;
            this.rand = rand;

            this.waitForExplosionTimer = new TickCounterStopwatch();
        }

        public bool IsExploded { get; private set; } = false;

        public void StartTimer(int durationInSeconds)
        {
            this.waitForExplosionTimer.StartTimer(ExplodeShip, durationInSeconds);
        }

        public void TimerTickHandler(float dt)
        {
            this.waitForExplosionTimer.TimerTickHandler(dt);

            this.particleHandler.HandleTimerTick(dt);            
        }

        public void DrawParticles(GraphicPanel2D panel)
        {
            this.particleHandler.DrawParticles(panel);
        }

        public void ExplodeShip()
        {
            if (this.IsExploded) return;

            this.IsExploded = true;            
            this.sounds.PlayExplosionSound();
            this.mainThruster.IsEnabled = false;
            
            int destroyMode = rand.Next(3);            
            switch (destroyMode)
            {
                case 0:
                    DestroyWithParticles(rand);
                    break;

                case 1:
                    DestroyWithBoxes(rand);
                    break;

                case 2:
                    DestroyWithVoronoi(rand);
                    break;
            }
        }

        private void ExamplesForUsingTheDestroyFunction()
        {
            this.ship = simulator.GetBodyByTagName("ship");

            //Bsp 1: Zerlege den Körper in Kästchen mit Defaultparametern
            this.simulator.DestroyRigidBody(this.ship, IRigidDestroyerParameter.DestroyMethod.Boxes);

            //Bsp 2: Unterteile das Objekt in 3*3 gleichgroße Rechtecke
            this.simulator.DestroyRigidBody(this.ship, new DestroyWithBoxesParameter() { BoxCount = 3 });

            //Bsp 3: Lass die Bruchstücke vom Zentrum wegschleudern
            this.simulator.DestroyRigidBody(this.ship, new DestroyWithBoxesParameter()
            {
                //body = this.ship; poly = Bruchstück/Kästchen was erzeugt wurde
                TransformFunc = (body, poly) =>
                {
                    poly.Velocity += poly.Center - body.Center;
                    return poly;
                }
            });
        }

        private void DestroyWithParticles(Random rand)
        {
            this.simulator.RemoveRigidBody(this.ship);
            this.particleHandler.AddParticles(ParticleHandler.CreateParticles((IPublicRigidRectangle)this.ship, 100, 1, Color.Red, Color.Yellow, rand));
        }

        private void DestroyWithBoxes(Random rand)
        {            
            this.simulator.DestroyRigidBody(this.ship, new DestroyWithBoxesParameter() 
            { 
                BoxCount = 5,
                TransformFunc = (body, polyObj) =>
                {
                    polyObj.Velocity += GetRandomVec(rand, 0.02f);
                    polyObj.AngularVelocity += (float)rand.NextDouble() * 0.1f;
                    return polyObj;
                },
            });
        }

        private void DestroyWithVoronoi(Random rand)
        {
            this.simulator.DestroyRigidBody(this.ship, new DestroyWithVoronoiParameter()
            {
                CellCount = 10,
                TransformFunc = (body, polyObj) =>
                {
                    polyObj.Velocity += GetRandomVec(rand, 0.02f);
                    polyObj.AngularVelocity += (float)rand.NextDouble() * 0.1f;
                    return polyObj;
                },
            });
        }

        private static Vec2D GetRandomVec(Random rand, float max)
        {
            return new Vec2D(GetRandomNumber(rand, max), GetRandomNumber(rand, max));
        }

        private static float GetRandomNumber(Random rand, float max)
        {
            return (float)rand.NextDouble() * max * 2 - max;
        }
    }
}
