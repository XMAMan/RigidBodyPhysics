namespace RigidBodyPhysics.MathHelper
{
    internal class Vec3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vec3D() { } //Wird für den Json-Converter benötigt

        public Vec2D XY
        {
            get => new Vec2D(X, Y);
        }

        public Vec3D(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vec3D(Vec3D copy)
        {
            this.X = copy.X;
            this.Y = copy.Y;
            this.Z = copy.Z;
        }

        public override string ToString()
        {
            return "[" + X.ToString() + ";" + Y.ToString() + ";" + Z.ToString() + "]";
        }

        public static Vec3D operator +(Vec3D v1, Vec3D v2)
        {
            return new Vec3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vec3D operator -(Vec3D v1, Vec3D v2)
        {
            return new Vec3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }
        public static Vec3D operator -(Vec3D v)
        {
            return -1 * v;
        }

        public static float operator *(Vec3D v1, Vec3D v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }
        public static Vec3D operator *(Vec3D v, float f)
        {
            return new Vec3D(v.X * f, v.Y * f, v.Z * f);
        }
        public static Vec3D operator *(float f, Vec3D v)
        {
            return new Vec3D(v.X * f, v.Y * f, v.Z * f);
        }
    }
}
