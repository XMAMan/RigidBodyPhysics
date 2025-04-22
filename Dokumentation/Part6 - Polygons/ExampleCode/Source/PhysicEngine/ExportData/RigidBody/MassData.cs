namespace PhysicEngine.ExportData.RigidBody
{
    public class MassData
    {
        public enum MassType { Mass, Density, Infinity }
        public MassType Type { get; set; } //Auf diese Weise möchte der Nutzer das Gewicht des Körpers festlegen
        public float Mass { get; set; } = 1;
        public float Density { get; set; } = 1;

        public MassData(MassType type, float mass, float density)
        {
            Type = type;
            Mass = mass;
            Density = density;
        }

        public float GetMass(float shapeArea)
        {
            switch (Type)
            {
                case MassType.Mass:
                    return Mass;

                case MassType.Density:
                    return shapeArea * Density;

                case MassType.Infinity:
                    return float.MaxValue;
            }

            return float.NaN;
        }

        public float GetDensity(float shapeArea)
        {
            switch (Type)
            {
                case MassType.Mass:
                    return Mass / shapeArea;

                case MassType.Density:
                    return Density;

                case MassType.Infinity:
                    return float.MaxValue;
            }

            return float.NaN;
        }
    }
}
