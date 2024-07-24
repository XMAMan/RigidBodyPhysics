namespace PhysicEngine.MathHelper
{
    public class Vec2D
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vec2D() { } //Wird für den Json-Converter benötigt

        public Vec2D(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public Vec2D(Vec2D copy)
        {
            this.X = copy.X;
            this.Y = copy.Y;
        }

        public override string ToString()
        {
            return "[" + X.ToString() + ";" + Y.ToString() + "]";
        }

        public static Vec2D operator +(Vec2D v1, Vec2D v2)
        {
            return new Vec2D(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vec2D operator -(Vec2D v1, Vec2D v2)
        {
            return new Vec2D(v1.X - v2.X, v1.Y - v2.Y);
        }
        public static Vec2D operator -(Vec2D v)
        {
            return -1 * v;
        }

        public static float operator *(Vec2D v1, Vec2D v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }
        public static Vec2D operator *(Vec2D v, float f)
        {
            return new Vec2D(v.X * f, v.Y * f);
        }
        public static Vec2D operator *(float f, Vec2D v)
        {
            return new Vec2D(v.X * f, v.Y * f);
        }

        public static Vec2D operator /(Vec2D v, float f)
        {
            return new Vec2D(v.X / f, v.Y / f);
        }

        public float Length()//Länge eines Vektors
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public float SquareLength() //Länge des Vektors ins Quadrat
        {
            return X * X + Y * Y;
        }

        public Vec2D Normalize()
        {
            return this / Length();
        }

        public Vec2D Abs()
        {
            return new Vec2D(Math.Abs(this.X), Math.Abs(this.Y));
        }

        public static float ZValueFromCross(Vec2D v1, Vec2D v2)
        {
            return v1.X * v2.Y - v2.X * v1.Y;
        }

        //Kreuzprodukt zwischen (v.X, v.Y, 0) und (0,0,z)
        public static Vec2D CrossWithZ(Vec2D v, float z)
        {
            return new Vec2D(z * v.Y, -z * v.X);
        }

        //Kreuzprodukt zwischen (0,0,z) und (v.X, v.Y, 0) 
        public static Vec2D CrossWithZ(float z, Vec2D v)
        {
            return new Vec2D(-z * v.Y, z * v.X);
        }

        public static Vec2D DirectionFromPhi(double phi)
        {
            return new Vec2D((float)Math.Cos(phi), (float)Math.Sin(phi));
        }

        public static Vec2D RotatePointAroundPivotPoint(Vec2D pivotpoint, Vec2D point, float angleInDegree)
        {
            double angleRadians = angleInDegree * Math.PI / 180;

            Vec2D v = point - pivotpoint;

            return new Vec2D(pivotpoint.X + (float)(Math.Cos(angleRadians) * v.X - Math.Sin(angleRadians) * v.Y),
                             pivotpoint.Y + (float)(Math.Sin(angleRadians) * v.X + Math.Cos(angleRadians) * v.Y));
        }

        public Vec2D SignZeroIsOne()
        {
            return new Vec2D(SignZeroIsOne(this.X), SignZeroIsOne(this.Y));
        }

        private static float SignZeroIsOne(float x)
        {
            return x < 0.0f ? -1.0f : 1.0f;
        }

        public Vec2D SignZeroIsZero()
        {
            return new Vec2D(SignZeroIsZero(this.X), SignZeroIsZero(this.Y));
        }

        private static float SignZeroIsZero(float x)
        {
            if (x == 0) return 0;
            return x < 0.0f ? -1.0f : 1.0f;
        }

        //v1 und v2 sind Richtungsvektoren. v1 wird gedanklich auf Einheitskreis-Startvektor (1,0) gelegt. 
        //v2 wird auch in den Kreis gelegt. Wenn v2.y > 0, dann liegt Winkel zwischen 0 und 180 Grad, sonst zwischen 180 und 360
        //Die Richtungsvektoren werden im 1. Quadrant angegeben. Die Zeichenfunktionen befinden sich aber im 4. Quadrant! Für den 4. Quadrant bitte Angle360YMirrored nehmen.
        public static float Angle360(Vec2D v1, Vec2D v2) //Winkel zwischen zwei Richtungsvektoren
        {
            v1 = v1.Normalize();
            v2 = v2.Normalize();

            float f1 = v1 * v2;
            if (f1 > 1) f1 = 1;
            if (f1 < -1) f1 = -1;
            float f = (float)(Math.Acos(f1) * 180 / Math.PI);

            Vec2D vD = v1.Spin90();

            float v2Y = vD.X * v2.X + vD.Y * v2.Y; //Das ist die Y-Koordinate, von vD, welche in den Einheitskreis projektziert wurde
            if (v2Y < 0) f = 360 - f;
            return f;
        }

        public static float Angle360YMirrored(Vec2D v1, Vec2D v2)
        {
            return Angle360(new Vec2D(v1.X, -v1.Y), new Vec2D(v2.X, -v2.Y));
        }

        //Wenn man im 4. Quadrant ist: Drehe 90 Grad nach Rechts (Im Uhrzeigersinn)
        public Vec2D Spin90()
        {
            return new Vec2D(-this.Y, this.X);
        }

        public static Vec2D Projection(Vec2D v1, Vec2D v2)//Projektziert v1 senkrecht auf v2
        {
            Vec2D ret = v2 * ((v1 * v2) / (v2 * v2));
            if (!float.IsNaN(ret.X) && !float.IsNaN(ret.Y)) return ret;
            return new Vec2D(0, 0);
        }

        //Diese Funktion ist das Gegenstück zu 'Angle360'. v1 wird um den Winkel 'angle360' gedreht, um v2 zu erhalten. Wenn angle360==0 ist, dann ist der Returnwert = v1
        public static Vec2D GetV2FromAngle360(Vec2D v1, float angle360)
        {
            float v1Length = v1.Length();
            v1 /= v1Length;
            Vec2D vD = v1.Spin90();

            float w = angle360 * (float)Math.PI / 180;
            Vec2D v2 = new Vec2D((float)Math.Cos(w), (float)Math.Sin(w));

            //Das ist die inverse/transpornierte Matrix von oben, um v2 aus dem Kreis-Koordinaten in Weltkoordinaten zu konvertieren
            float[] circleMatrix = new float[]{v1.X, v1.Y,
                                               vD.X, vD.Y};
            Vec2D v2W = new Vec2D(circleMatrix[0] * v2.X + circleMatrix[2] * v2.Y,
                                        circleMatrix[1] * v2.X + circleMatrix[3] * v2.Y);

            return v2W * v1Length;
        }
    }
}
