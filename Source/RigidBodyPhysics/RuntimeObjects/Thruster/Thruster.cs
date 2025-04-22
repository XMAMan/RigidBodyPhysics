using RigidBodyPhysics.ExportData.Thruster;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.RuntimeObjects.Thruster
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

        private bool isEnabled = false;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    IsEnabledChanged?.Invoke(value);
                }
            }
        }
        public event Action<bool> IsEnabledChanged;
        #endregion


        #region IThruster        
        public IRigidBody B1 { get; private set; }
        public Vec2D R1 { get; private set; } //Angabe in Weltkoordinaten (Hebelarm vom Center zum Anchor-Punkt)    
        public void UpdateAnchorPoints() //Muss aufgerufen werden, wenn sich die Position der Bodys geändert hat
        {
            Anchor = MathHelp.GetWorldPointFromLocalDirection(B1, r1);
            R1 = Anchor - B1.Center;
            ForceDirection = MathHelp.GetWorldDirectionFromLocalDirection(B1, forceDirection);
        }
        #endregion

        public Thruster(ThrusterExportData data, List<IRigidBody> bodies)
        {
            Body = B1 = bodies[data.BodyIndex];
            r1 = data.R1;
            forceDirection = data.ForceDirection;
            ForceLength = data.ForceLength;
            IsEnabled = data.IsEnabled;

            UpdateAnchorPoints();
        }

        #region IExportableThruster
        public IExportThruster GetExportData(List<IRigidBody> bodies)
        {
            return new ThrusterExportData()
            {
                BodyIndex = bodies.IndexOf(B1),
                R1 = r1,
                ForceDirection = forceDirection,
                ForceLength = ForceLength,
                IsEnabled = IsEnabled
            };
        }
        #endregion
    }
}
