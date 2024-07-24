using Part1.ViewModel.Editor;

namespace Part1.Model.Editor.EditorShape
{
    static class MassHelper
    {
        public static float GetMass(MassType massType, float mass, float densitiy, float shapeArea)
        {
            switch (massType)
            {
                case MassType.Mass:
                    return mass;

                case MassType.Density:
                    return shapeArea * densitiy;

                case MassType.Infinity:
                    return float.MaxValue;
            }

            return float.NaN;
        }
    }
}
