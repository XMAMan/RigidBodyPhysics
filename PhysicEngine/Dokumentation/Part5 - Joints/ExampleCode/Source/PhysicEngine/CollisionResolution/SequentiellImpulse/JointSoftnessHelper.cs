using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse
{
    static internal class JointSoftnessHelper
    {
        //Wenn ich bei einer Feder vorgeben will, mit welcher Frequenz sie schwingt, dann muss die Feder-Steifheit+Dämpfung 
        //von der Masse abhängen welche an der Feder hängt. Diese Formeln hier helfen beim Errechnen der Stiffness und Damping-Werte
        //Quelle für die Formeln: https://github.com/erincatto/box2d/blob/411acc32eb6d4f2e96fc70ddbdf01fe5f9b16230/src/dynamics/b2_joint.cpp#L40
        //Weitere Quelle: https://box2d.org/files/ErinCatto_SoftConstraints_GDC2011.pdf Seite 45
        internal static void LinearFrequencyToStiffness(float frequencyHertz, float dampingRatio, IRigidBody bodyA, IRigidBody bodyB, out float stiffness, out float damping)
        {
            float massA = (bodyA != null && bodyA.InverseMass != 0) ? 1.0f / bodyA.InverseMass : 0;
            float massB = (bodyB != null && bodyB.InverseMass != 0) ? 1.0f / bodyB.InverseMass : 0;
            float mass;
            if (massA >0 && massB >0)
            {
                mass = massA * massB / (massA + massB);
            }else if (massA > 0)
            {
                mass = massA;
            }else
            {
                mass = massB;
            }

            //Mit der 1000-Division rechne ich die Frequenc-Pro-Millisekundenzahl in eine Frequence-Pro-Sekunden-Zahl um
            float omega = 2 * (float)Math.PI * frequencyHertz / 1000; 
            stiffness = mass * omega * omega;           //k
            damping = 2 * mass * dampingRatio * omega;  //c
        }

        internal static void LinearStiffnessToFrequency(float stiffness, float damping, IRigidBody bodyA, IRigidBody bodyB, out float frequencyHertz, out float dampingRatio)
        {
            float massA = (bodyA != null && bodyA.InverseMass != 0) ? 1.0f / bodyA.InverseMass : 0;
            float massB = (bodyB != null && bodyB.InverseMass != 0) ? 1.0f / bodyB.InverseMass : 0;
            float mass;
            if (massA > 0 && massB > 0)
            {
                mass = massA * massB / (massA + massB);
            }
            else if (massA > 0)
            {
                mass = massA;
            }
            else
            {
                mass = massB;
            }

            float omega = (float)Math.Sqrt(stiffness / mass);
            frequencyHertz = omega * 1000 / (2 * (float)Math.PI);
            dampingRatio = damping / (2 * mass * omega);
        }
    }
}
