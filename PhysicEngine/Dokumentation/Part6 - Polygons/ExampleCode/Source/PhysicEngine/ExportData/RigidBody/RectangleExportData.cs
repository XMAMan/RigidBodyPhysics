﻿using PhysicEngine.MathHelper;

namespace PhysicEngine.ExportData.RigidBody
{
    public class RectangleExportData : PropertysExportData, IExportRigidBody
    {
        public Vec2D Size { get; set; }
        public bool BreakWhenMaxPushPullForceIsReached { get; set; }
        public float MaxPushPullForce { get; set; }
    }
}