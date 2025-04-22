using GraphicMinimal;

namespace GameHelper
{
    public static class ForceColorCreator
    {
        //minForce = -0.0001f .. beliebig negative Zahl
        //maxForce = 0.0001f .. beliebig positive Zahl
        public static Vector3D GetForceColor(float force,float minForce, float maxForce)
        {
            //f=-1 Maximal Zug; 0=Keine Zug/Druck; 1=Maximal Druck
            float f = 0;

            if (force >= 0)
            {
                f = Math.Min(1, Math.Max(-1, force / Math.Max(0.0001f, maxForce)));
            }
            else
            {
                f = Math.Min(1, Math.Max(-1, -force / Math.Min(-0.0001f, minForce)));
            }

            if (f == 0) return new GraphicMinimal.Vector3D(0, 1, 0);

            GraphicMinimal.Vector3D color;
            if (f < 0)
            {
                var c1 = new GraphicMinimal.Vector3D(0, 0, 1);  //Maximal Zug = Blau
                var c2 = new GraphicMinimal.Vector3D(0, 1, 0);  //Keine Kraft = Grün
                f += 1;
                color = (1 - f) * c1 + f * c2;

            }
            else
            {
                var c1 = new GraphicMinimal.Vector3D(0, 1, 0);  //Keine Kraft = Grün
                var c2 = new GraphicMinimal.Vector3D(1, 0, 0);  //Maximal Druck = Rot
                color = (1 - f) * c1 + f * c2;
            }

            return color;
        }
    }
}
