using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix
{
    //Konvertiert eine Liste von Bodies in Matrizen
    internal static class BodiesToMatrix
    {
        public static Matrix GetV(List<IRigidBody> bodies)
        {
            float[] v = new float[bodies.Count * 3];

            for (int i = 0; i < bodies.Count; i++)
            {
                v[i * 3 + 0] = bodies[i].Velocity.X;
                v[i * 3 + 1] = bodies[i].Velocity.Y;
                v[i * 3 + 2] = bodies[i].AngularVelocity;
            }

            return Matrix.GetColumVector(v);
        }

        public static Matrix GetFext(List<IRigidBody> bodies)
        {
            float[] v = new float[bodies.Count * 3];

            for (int i = 0; i < bodies.Count; i++)
            {
                v[i * 3 + 0] = bodies[i].Force.X;
                v[i * 3 + 1] = bodies[i].Force.Y;
                v[i * 3 + 2] = bodies[i].Torque;
            }

            return Matrix.GetColumVector(v);
        }

        public static Matrix GetInverseMassMatrix(List<IRigidBody> bodies)
        {
            float[,] m = new float[bodies.Count * 3, bodies.Count * 3];

            for (int i = 0;i < bodies.Count; i++)
            {
                m[i * 3 + 0, i * 3 + 0] = bodies[i].InverseMass;
                m[i * 3 + 1, i * 3 + 1] = bodies[i].InverseMass;
                m[i * 3 + 2, i * 3 + 2] = bodies[i].InverseInertia;
            }

            return new Matrix(m);
        }
    }
}
