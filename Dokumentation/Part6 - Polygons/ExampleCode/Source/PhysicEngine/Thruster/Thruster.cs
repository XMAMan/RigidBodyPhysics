using PhysicEngine.ExportData.Thruster;
using PhysicEngine.MathHelper;
using PhysicEngine.RigidBody;

namespace PhysicEngine.Thruster
{
    //Schubdüse welche über externe Kraft arbeitet
    internal class Thruster : IThruster
    {
        private Vec2D r1; //lokaler Richtungsvektor von B1.Center nach Anchor
        private Vec2D forceDirection; //Lokaler Richtungsvektor

        #region IPublicThruster
        public IPublicRigidBody Body { get; private set; }
        public Vec2D Anchor { get; private set; }
        public Vec2D ForceDirection { get; private set; } //Richtungsvektor im Globalspace
        public float ForceLength { get; set; }
        public bool IsEnabled { get; set; }
        #endregion


        #region IThruster        
        public IRigidBody B1 { get; private set; }
        public Vec2D R1 { get; private set; } //Angabe in Weltkoordinaten (Hebelarm vom Center zum Anchor-Punkt)
        public Vec2D Force { get; private set; } //Angabe in Weltkoordinaten        
        public void UpdateAnchorPoints() //Muss aufgerufen werden, wenn sich die Position der Bodys geändert hat
        {
            this.Anchor = MathHelp.GetWorldPointFromLocalDirection(this.B1, this.r1);
            this.R1 = this.Anchor - this.B1.Center;
            this.ForceDirection = MathHelp.GetWorldDirectionFromLocalDirection(this.B1, this.forceDirection);
            this.Force = this.ForceDirection * this.ForceLength;
        }
        #endregion

        public Thruster(ThrusterExportData data, List<IRigidBody> bodies)
        {
            this.Body = this.B1 = bodies[data.BodyIndex];
            this.r1 = data.R1;
            this.forceDirection = data.ForceDirection;
            this.ForceLength = data.ForceLength;
            this.IsEnabled = data.IsEnabled;

            UpdateAnchorPoints();
        }

        #region IExportableThruster
        public IExportThruster GetExportData(List<IRigidBody> bodies)
        {
            return new ThrusterExportData()
            {
                BodyIndex = bodies.IndexOf(B1),
                R1 = this.r1,
                ForceDirection = this.forceDirection,
                ForceLength = this.ForceLength,
                IsEnabled = this.IsEnabled
            };
        }
        #endregion
    }
}
