using System;
using GameHelper.Simulation.RigidBodyTagging;
using GameHelper.Simulation;
using RigidBodyPhysics.CollisionDetection;
using RigidBodyPhysics;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace SkiJumperControl.Model
{
    internal class Skiman
    {
        private IPublicJoint weldPlankBack, weldPlankFront, weldHead, weldSkiTip;
        private IPublicRigidBody leg;

        private ITagDataProvider tagDataProvider;
        private Sounds sounds;

        public event Action WasHurtHandler; //Event was getriggert wird, wenn der Skifahrer sich verletzt hat
        public event Action FlagWasReached; //Event wird getriggert, wenn der Skifahrer die Fahne berührt

        public Skiman(GameSimulator simulator, Sounds sounds)
        {
            this.tagDataProvider = simulator;
            this.sounds = sounds;

            this.weldPlankBack = simulator.GetJointByTagName("weldPlankBack"); //Ski-Brett hinten
            this.weldPlankFront = simulator.GetJointByTagName("weldPlankFront"); //Ski-Brett vorne
            this.weldHead = simulator.GetJointByTagName("weldHead"); //Kopf
            this.weldSkiTip = simulator.GetJointByTagName("weldSkiTip"); //Skispitze
            this.leg = simulator.GetBodyByTagName("leg");

            simulator.JointWasDeletedHandler += Simulator_JointWasDeletedHandler;

            simulator.CollisonOccured += Simulator_CollisonOccured;
        }

        private void Simulator_CollisonOccured(PhysicScene sender, PublicRigidBodyCollision[] collisions)
        {
            bool manIsTouchingTheGround = false;
            bool callFlagIsTouched = false;
            bool boardIsTouchingTheGround = false;

            foreach (var collision in collisions)
            {
                byte color1 = this.tagDataProvider.GetTagDataFromBody(collision.Body1).Color;
                byte color2 = this.tagDataProvider.GetTagDataFromBody(collision.Body2).Color;
                
                //Tag-Color-Werte:
                //0 = Boden
                //1 = Skifahrer ohne Brett
                //2 = Skibrett
                //3 = Flagge

                //Skifahrer berührt Fahne
                if ((color1 == 1 || color1 == 2) && color2 == 3)
                {
                    callFlagIsTouched = true;
                }

                //Skifahrer berührt mit sein Körper den Boden
                if (color1 == 1 && color2 == 0)
                {
                    manIsTouchingTheGround = true;
                }

                //Skibrett berührt den Boden
                if (color1 == 2 && color2 == 0)
                {
                    boardIsTouchingTheGround = true;
                }
            }

            this.FlagIsTouched = callFlagIsTouched;
            this.ManIsTouchingTheGround = manIsTouchingTheGround;
            this.BoardIsTouchingTheGround = boardIsTouchingTheGround;

            if (callFlagIsTouched)
            {
                if (GetFlagIsReachedWithoutInjury())
                {
                    this.FlagIsReachedWithoutInjury = true;
                    this.sounds.PlayYes();
                }else if (GetFlagIsReachedWithInjury())
                {
                    //this.sounds.PlayOhNo();
                }
                
                this.FlagWasReached?.Invoke();
            }
        }

        private void Simulator_JointWasDeletedHandler(RigidBodyPhysics.PhysicScene sender, IPublicJoint joint)
        {
            if (this.FlagIsReachedWithoutInjury == false)
            {
                if (joint == weldPlankBack || joint == weldPlankFront)
                {
                    this.sounds.PlayBrokePlank();
                }

                if (joint == weldHead)
                {
                    this.sounds.PlayOuch();
                }
            }            

            this.WasHurtHandler?.Invoke();
        }

        public bool SkiBoardIsBroken { get => this.weldPlankBack.IsBroken || this.weldPlankFront.IsBroken; }
        public bool SkimanWasInjured { get => this.weldHead.IsBroken; }
        public bool FlagIsTouched { get; private set; } = false;
        public bool ManIsTouchingTheGround { get; private set; } = false; //Berührt der Fahrer mit was anderen als den Brettern den Boden?
        public bool BoardIsTouchingTheGround { get; private set; } = false;//Berühren die Skibretter den Boden?
        public bool FlagIsReachedWithoutInjury { get; private set; } = false;

        private bool GetFlagIsReachedWithoutInjury()
        {
            return this.FlagIsTouched && this.SkimanWasInjured == false && this.SkiBoardIsBroken == false && this.ManIsTouchingTheGround == false && this.BoardIsTouchingTheGround;
        }

        private bool GetFlagIsReachedWithInjury()
        {
            return this.FlagIsTouched && (this.SkimanWasInjured || this.ManIsTouchingTheGround);
        }

        public enum Somersault
        {
            Nothing, Backflip, DoubleBackflip, TrippleBackflip, Frontflip, DoubleFrontflip, TrippleFrontflip, Unknown
        }
        public Somersault GetSomersaults()
        {
            double angle = this.leg.Angle / (2 * Math.PI);

            double min = -0.2, max = 0.2;

            if (angle > min && angle < max) return Somersault.Nothing;
            if (angle > (min - 1) && angle < (max - 1)) return Somersault.Backflip;
            if (angle > (min - 2) && angle < (max - 2)) return Somersault.DoubleBackflip;
            if (angle > (min - 3) && angle < (max - 3)) return Somersault.TrippleBackflip;
            if (angle > (min + 1) && angle < (max + 1)) return Somersault.Frontflip;
            if (angle > (min + 2) && angle < (max + 2)) return Somersault.DoubleFrontflip;
            if (angle > (min + 3) && angle < (max + 3)) return Somersault.TrippleFrontflip;

            return Somersault.Unknown;
        }
    }
}
