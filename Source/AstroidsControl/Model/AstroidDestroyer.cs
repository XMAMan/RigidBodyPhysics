using DynamicObjCreation.RigidBodyDestroying;
using GameHelper.ParticleHandling;
using GameHelper.Simulation;
using GameHelper.Simulation.RigidBodyTagging;
using GraphicPanels;
using GraphicPanelWpf;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System;
using System.Drawing;
using System.Linq;

namespace AstroidsControl.Model
{
    //Zerstört die Astroiden und den Satellit, wenn er vom Schuss berührt wird
    class AstroidDestroyer : ITimerHandler
    {
        private GameSimulator simulator;
        private Sounds sounds;
        private Random rand;

        private BodyIsInsideScreenTester insideScreenTester;
        private ParticleHandler particleHandler = new ParticleHandler();

        public int DestroyCounter { get; private set; } = 0;

        public AstroidDestroyer(GameSimulator simulator, Sounds sounds, Random rand)
        {
            this.simulator = simulator;
            this.sounds = sounds;
            this.rand = rand;
            this.insideScreenTester = new BodyIsInsideScreenTester(simulator);

            simulator.TagOrderedCollisonOccured += Simulator_CollisonOccured;            
        }

        public void HandleTimerTick(float dt)
        {
            this.insideScreenTester.HandleTimerTick(dt);
            this.particleHandler.HandleTimerTick(dt);
        }

        public void Draw(GraphicPanel2D panel)
        {
            this.particleHandler.DrawParticles(panel);
        }


        private void Simulator_CollisonOccured(GameSimulator sender, TagColorOrderedCollisionEvent[] collisions)
        {
            foreach (var c in collisions)
            {
                //0=Ship;1=Bullet;2=Astroid;3=Satellit

                //Schuss berührt Astroid
                if (c.Color1 == 1 && c.Color2 == 2)
                {
                    this.simulator.RemoveRigidBody(c.Body1); //Entferne den Schuss
                    
                    if (c.Body2.Area < 10000) //Es wurde ein kleiner Stein getroffen -> Erzeuge Partikel
                    {
                        this.simulator.RemoveRigidBody(c.Body2); //Entferne den Astroid
                        this.particleHandler.AddParticles(ParticleHandler.CreateParticles((IPublicRigidPolygon)c.Body2, 100, 2, Color.Gray, Color.Red, this.rand));
                        this.DestroyCounter++;
                        this.sounds.PlayParticleExplode();
                    }
                    else //Zerlege den großen Stein ein lauter kleinere
                    {
                        var bodies = this.simulator.DestroyRigidBody(c.Body2, new DestroyWithVoronoiParameter()
                        {
                            CellCount = 10,
                            TransformFunc = (body, poly) =>
                            {
                                poly.Velocity += (poly.Center - body.Center) * 0.002f * (float)rand.NextDouble();
                                return poly;
                            }
                        });

                        foreach (var body in bodies)
                        {
                            this.insideScreenTester.AddBody(body);
                        }

                        this.sounds.PlayExplodeAstroid();
                    }

                    

                    break; //Abbruch da es mehrere Kollisionspunkte zwischen den Schuss und dem Astroid geben kann
                }

                //Schuss berührt Satellit
                if (c.Color1 == 1 && c.Color2 == 3)
                {
                    this.simulator.RemoveRigidBody(c.Body1); //Entferne den Schuss



                    int satellitId = this.simulator.GetTagDataFromBody(c.Body2).LevelItemId;
                    var bodies = this.simulator.GetBodiesByTagName(satellitId, "satellit").ToList();
                    
                    if (bodies.Count == 1)
                    {
                        this.particleHandler.AddParticles(ParticleHandler.CreateParticles(c.Body2, 100, 2, Color.Gray, Color.Red, this.rand));
                        this.DestroyCounter++;
                        this.sounds.PlayParticleExplode();
                    }
                    else
                    {
                        foreach (var body in bodies)
                        {
                            var texture = this.simulator.GetTextureDataFromBody(body);

                            //Beim Satelliten gibt es ein Körper, wo IsInvisible true ist. Dieser soll nicht zerlegt werden
                            if (texture == null || texture.IsInvisible)
                            {
                                this.simulator.RemoveRigidBody(body);
                            }
                            else
                            {
                                var destroyBodies = this.simulator.DestroyRigidBody(body, new DestroyWithBoxesParameter()
                                {
                                    BoxCount = 3,
                                });

                                foreach (var destroy in destroyBodies)
                                {
                                    this.insideScreenTester.AddBody(destroy);
                                }

                                this.sounds.PlayExplodeSatellit();
                            }

                        }
                    }

                    
                    this.simulator.RemoveLevelItem(satellitId);

                    

                    break; //Abbruch da es mehrere Kollisionspunkte zwischen den Schuss und dem Satellit geben kann
                }
            }            
        }

    }
}
