namespace PhysicEngine.ExportData
{
    public class MassData
    {
        public enum MassType { Mass, Density, Infinity }
        public MassType Type { get; set; }
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
            switch (this.Type)
            {
                case MassType.Mass:
                    return this.Mass;

                case MassType.Density:
                    return shapeArea * this.Density;

                case MassType.Infinity:
                    return float.MaxValue;
            }

            return float.NaN;
        }
    }
}
