using GameHelper.Simulation;
using GraphicPanelWpf;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace AstroidsControl.Model
{
    //Prüft für eine Menge von RigidBodys ob sie innerhalb des Sichtfeldes sind und löscht sie wenn sie außerhalb sind
    class BodyIsInsideScreenTester : ITimerHandler
    {
        private GameSimulator simulator;

        private List<IPublicRigidBody> bodies = new List<IPublicRigidBody>();
        private int border = 0; //>0 = Das Rechteck ist größer als der Bildschirm

        public event Action<IPublicRigidBody> BodyWasDeletedHandler;

        public BodyIsInsideScreenTester(GameSimulator simulator, int border = 0)
        {
            this.simulator = simulator;
            this.border = border;
            this.simulator.BodyWasDeletedHandler += Simulator_BodyWasDeletedHandler;
        }

        private void Simulator_BodyWasDeletedHandler(RigidBodyPhysics.PhysicScene sender, IPublicRigidBody body)
        {
            if (this.bodies.Contains(body))
            {
                this.bodies.Remove(body);                
            }
        }

        public int Count { get { return bodies.Count; } }


        public void AddBody(IPublicRigidBody body)
        {
            this.bodies.Add(body);
        }

        public void HandleTimerTick(float dt)
        {
            var box = this.simulator.GetScreenBox();
            List<IPublicRigidBody> delList = new List<IPublicRigidBody>();
            foreach (var body in this.bodies)
            {
                if (IsInsideRectangle(body, box) == false)
                {
                    delList.Add(body);
                }
            }
            foreach (var del in delList)
            {
                this.BodyWasDeletedHandler?.Invoke(del);
                this.bodies.Remove(del);
                this.simulator.RemoveRigidBody(del);
            }
        }

        private bool IsInsideRectangle(IPublicRigidBody body, RectangleF box)
        {
            return body.Center.X > box.Left - border &&
                body.Center.X < box.Right + border &&
                body.Center.Y > box.Top - border &&
                body.Center.Y < box.Bottom + border;
        }
    }
}
