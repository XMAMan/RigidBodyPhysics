﻿using GraphicPanels;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorShape
{
    internal class SimulatorCircle : ISimulatorShape
    {
        private IPublicRigidCircle rigidCircle;

        public IPublicRigidBody PhysicModel { get; private set; }

        public SimulatorCircle(IPublicRigidCircle ctor)
        {
            this.PhysicModel = this.rigidCircle = ctor;
        }

        public void Draw(GraphicPanel2D panel, PrintSettingsViewModel printSettings)
        {
            var c = this.rigidCircle;
            panel.DrawCircle(Pens.Black, c.Center.ToGrx(), c.Radius);

            Vec2D r = Vec2D.DirectionFromPhi(c.Angle);
            panel.DrawLine(Pens.Black, c.Center.ToGrx(), (c.Center + r * c.Radius).ToGrx());
        }
    }
}