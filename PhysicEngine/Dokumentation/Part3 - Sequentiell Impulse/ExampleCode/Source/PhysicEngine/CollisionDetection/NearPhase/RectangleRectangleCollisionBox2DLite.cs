using GraphicMinimal;
using PhysicEngine.MathHelper;
using System.Runtime.InteropServices;

namespace PhysicEngine.CollisionDetection.NearPhase
{
    //Quelle: https://github.com/erincatto/box2d-lite/blob/master/src/Collide.cpp
    public static class RectangleRectangleCollisionBox2DLite
    {
        // Box vertex and edge numbering:
        //
        //        ^ y
        //        |
        //        e1
        //   v2 ------ v1
        //    |        |
        // e2 |        | e4  --> x
        //    |        |
        //   v3 ------ v4
        //      

        enum Axis
        {
            FACE_A_X,
            FACE_A_Y,
            FACE_B_X,
            FACE_B_Y
        }

        enum EdgeNumbers
        {
            NO_EDGE = 0,
            EDGE1,
            EDGE2,
            EDGE3,
            EDGE4
        }

        struct ClipVertex
        {
            public ClipVertex()
            {
                this.v = null;
                this.fp.Value = 0;
            }
            public Vector2D v;
            public FeaturePair fp;
        }

        [StructLayout(LayoutKind.Explicit)] //Damit kann ich eine Union nachbauen. Quelle: https://www.aspheute.com/artikel/20020207.htm
        struct FeaturePair
        {
            //Bytes 0..3
            [FieldOffset(0)] public byte inEdge1;
            [FieldOffset(1)] public byte outEdge1;
            [FieldOffset(2)] public byte inEdge2;
            [FieldOffset(3)] public byte outEdge2;

            //Bytes 0..3
            [FieldOffset(0)] public UInt32 Value;
        }

        private static void Swap(ref byte b1, ref byte b2)
        {
            byte tmp = b1;
            b1 = b2;
            b2 = tmp;
        }

        private static float Sign(float x)
        {
            return x < 0.0f ? -1.0f : 1.0f;
        }

        private static void Flip(ref FeaturePair fp)
        {
            Swap(ref fp.inEdge1, ref fp.inEdge2);
            Swap(ref fp.outEdge1, ref fp.outEdge2);
        }

        private static int ClipSegmentToLine(ClipVertex[] vOut, ClipVertex[] vIn, Vector2D normal, float offset, byte clipEdge)
        {
            // Start with no output points
            int numOut = 0;

            // Calculate the distance of end points to the line
            float distance0 = normal * vIn[0].v - offset;
            float distance1 = normal * vIn[1].v - offset;

            // If the points are behind the plane
            if (distance0 <= 0.0f) vOut[numOut++] = vIn[0];
            if (distance1 <= 0.0f) vOut[numOut++] = vIn[1];

            // If the points are on different sides of the plane
            if (distance0 * distance1 < 0.0f)
            {
                // Find intersection point of edge and plane
                // Try to "walk" down the vector by the amount of d0, and if your still not inside the box
                // discard the point, otherwise keep the point
                float interp = distance0 / (distance0 - distance1);
                vOut[numOut].v = vIn[0].v + interp * (vIn[1].v - vIn[0].v);
                if (distance0 > 0.0f)
                {
                    vOut[numOut].fp = vIn[0].fp;
                    vOut[numOut].fp.inEdge1 = clipEdge;
                    vOut[numOut].fp.inEdge2 = (byte)EdgeNumbers.NO_EDGE;
                }
                else
                {
                    vOut[numOut].fp = vIn[1].fp;
                    vOut[numOut].fp.outEdge1 = clipEdge;
                    vOut[numOut].fp.outEdge2 = (byte)EdgeNumbers.NO_EDGE;
                }
                ++numOut;
            }

            return numOut;
        }

        private static void ComputeIncidentEdge(ClipVertex[] c, Vector2D h, Vector2D pos, Matrix2x2 rot, Vector2D normal)
        {
            // The normal is from the reference box. Convert it
            // to the incident boxe's frame and flip sign.

            // In other words, Rotate the normal to the other box's local space
            Matrix2x2 rotT = rot.Transpose();
            Vector2D n = -(rotT * normal);
            Vector2D nAbs = n.Abs();

            // Figure out which edges correspond with the vertices that could possibly
            // be penetrating the box. So each incidentEdge has two edges which leads to one
            // possible vertex being inside the other box. 
            if (nAbs.X > nAbs.Y)
            {
                if (Sign(n.X) > 0.0f)
                {
                    c[0].v = new Vector2D(h.X, -h.Y);
                    c[0].fp.inEdge2 = (byte)EdgeNumbers.EDGE3;
                    c[0].fp.outEdge2 = (byte)EdgeNumbers.EDGE4;

                    c[1].v = new Vector2D(h.X, h.Y);
                    c[1].fp.inEdge2 = (byte)EdgeNumbers.EDGE4;
                    c[1].fp.outEdge2 = (byte)EdgeNumbers.EDGE1;
                }
                else
                {
                    c[0].v = new Vector2D(-h.X, h.Y);
                    c[0].fp.inEdge2 = (byte)EdgeNumbers.EDGE1;
                    c[0].fp.outEdge2 = (byte)EdgeNumbers.EDGE2;

                    c[1].v = new Vector2D(-h.X, -h.Y);
                    c[1].fp.inEdge2 = (byte)EdgeNumbers.EDGE2;
                    c[1].fp.outEdge2 = (byte)EdgeNumbers.EDGE3;
                }
            }
            else
            {
                if (Sign(n.Y) > 0.0f)
                {
                    c[0].v = new Vector2D(h.X, h.Y);
                    c[0].fp.inEdge2 = (byte)EdgeNumbers.EDGE4;
                    c[0].fp.outEdge2 = (byte)EdgeNumbers.EDGE1;

                    c[1].v = new Vector2D(-h.X, h.Y);
                    c[1].fp.inEdge2 = (byte)EdgeNumbers.EDGE1;
                    c[1].fp.outEdge2 = (byte)EdgeNumbers.EDGE2;
                }
                else
                {
                    c[0].v = new Vector2D(-h.X, -h.Y);
                    c[0].fp.inEdge2 = (byte)EdgeNumbers.EDGE2;
                    c[0].fp.outEdge2 = (byte)EdgeNumbers.EDGE3;

                    c[1].v = new Vector2D(h.X, -h.Y);
                    c[1].fp.inEdge2 = (byte)EdgeNumbers.EDGE3;
                    c[1].fp.outEdge2 = (byte)EdgeNumbers.EDGE4;
                }
            }

            c[0].v = pos + rot * c[0].v;
            c[1].v = pos + rot * c[1].v;
        }

        // The normal points from A to B
        public static CollisionInfo[] RectangleRectangle(ICollidableRectangle bodyA, ICollidableRectangle bodyB)
        {
            // Setup
            Vector2D hA = 0.5f * bodyA.Size;
            Vector2D hB = 0.5f * bodyB.Size;

            Vector2D posA = bodyA.Center;
            Vector2D posB = bodyB.Center;

            //Local space von einer Box = Ich tue so als ob die Box nicht gedreht wurde
            //Global space einer Box = Box wurde gedreht

            //Die erste Zeile der Rot-Matrix zeigt im local space (1,0) und im global space so wie die Box gedreht ist
            //Die zweite Zeile der Rot-Matrix zeigt im local space (0,1) im im global space so wie die Box gedreht ist
            Matrix2x2 rotA = Matrix2x2.Rotate(bodyA.Angle);
            Matrix2x2 rotB = Matrix2x2.Rotate(bodyB.Angle);

            Matrix2x2 rotAT = rotA.Transpose(); //Rotate anything into A's local space
            Matrix2x2 rotBT = rotB.Transpose(); //Rotate anything into B's local space

            Vector2D dp = posB - posA;          //Vektor vom Mittelpunkt von A nach Mittelpunkt von B
            Vector2D dA = rotAT * dp;           //dp wird in local space von A gedreht
            Vector2D dB = rotBT * dp;           //dp wird in local space von B gedreht

            Matrix2x2 C = rotAT * rotB;         //Rotiert alles von B's local space in A's local space
            Matrix2x2 absC = C.Abs();
            Matrix2x2 absCT = absC.Transpose(); //Rotiere alles von A's local space in B's local space

            //SAT Separating Axis Theorem
            // Box A faces
            // This does a dot product by using the transformation matrix and
            // matrix multiplicaiton rules. Very condensed...
            // So it's just projecting along all the faces
            //faceA.x = Wie weit dringt B in A ein (Beide Boxen wurden auf die X-Achse projetziert). Minuszahl heißt es wurde eingedrungen
            //faceA.y = Wie weit dringt B in A ein (Beide Boxen wurden auf die Y-Achse projetziert). Minuszahl heißt es wurde eingedrungen
            Vector2D faceA = dA.Abs() - hA - absC * hB;
            if (faceA.X > 0.0f || faceA.Y > 0.0f) //Es gab keine Kollision da Eindringtiefe > 0 (Verdrehtes Vorzeichen) ist
                return null;

            // Box B faces
            // Box B faces
            Vector2D faceB = dB.Abs() - absCT * hA - hB;//Wie weit dringt A in B ein (Minus heißt es dringt ein)
            if (faceB.X > 0.0f || faceB.Y > 0.0f)
                return null;

            // Find best axis
            Axis axis;
            float separation; //Eindringtiefe
            Vector2D normal;    //Eine von den 4 Flächennormalen von einer der beiden Boxen

            // Box A faces
            axis = Axis.FACE_A_X;
            separation = faceA.X;
            normal = dA.X > 0.0f ? rotA.Col1 : -rotA.Col1;

            const float relativeTol = 0.95f;
            const float absoluteTol = 0.01f;

            if (faceA.Y > relativeTol * separation + absoluteTol * hA.Y)
            {
                axis = Axis.FACE_A_Y;
                separation = faceA.Y;
                normal = dA.Y > 0.0f ? rotA.Col2 : -rotA.Col2;
            }

            // Box B faces
            if (faceB.X > relativeTol * separation + absoluteTol * hB.X)
            {
                axis = Axis.FACE_B_X;
                separation = faceB.X;
                normal = dB.X > 0.0f ? rotB.Col1 : -rotB.Col1;
            }

            if (faceB.Y > relativeTol * separation + absoluteTol * hB.Y)
            {
                axis = Axis.FACE_B_Y;
                separation = faceB.Y;
                normal = dB.Y > 0.0f ? rotB.Col2 : -rotB.Col2;
            }

            // Setup clipping plane data based on the separating axis

            // The front normal will be the normal in the direction of the collision
            // The side normal is perpendicular to the front normal
            // The incident edges are the two eges that are where the collision happend

            Vector2D frontNormal = null, sideNormal = null;
            ClipVertex[] incidentEdge = new ClipVertex[2];
            float front = float.NaN, negSide = float.NaN, posSide = float.NaN;
            byte negEdge = 255, posEdge = 255;

            // Compute the clipping lines and the line segment to be clipped.
            switch (axis)
            {
                case Axis.FACE_A_X:
                    {
                        frontNormal = normal;
                        front = (posA * frontNormal) + hA.X;
                        sideNormal = rotA.Col2;
                        float side = (posA * sideNormal);
                        negSide = -side + hA.Y;
                        posSide = side + hA.Y;
                        negEdge = (byte)EdgeNumbers.EDGE3;
                        posEdge = (byte)EdgeNumbers.EDGE1;
                        ComputeIncidentEdge(incidentEdge, hB, posB, rotB, frontNormal);
                    }
                    break;

                case Axis.FACE_A_Y:
                    {
                        frontNormal = normal;
                        front = (posA * frontNormal) + hA.Y;
                        sideNormal = rotA.Col1;
                        float side = (posA * sideNormal);
                        negSide = -side + hA.X;
                        posSide = side + hA.X;
                        negEdge = (byte)EdgeNumbers.EDGE2;
                        posEdge = (byte)EdgeNumbers.EDGE4;
                        ComputeIncidentEdge(incidentEdge, hB, posB, rotB, frontNormal);
                    }
                    break;

                case Axis.FACE_B_X:
                    {
                        frontNormal = -normal;
                        front = (posB * frontNormal) + hB.X;
                        sideNormal = rotB.Col2;
                        float side = (posB * sideNormal);
                        negSide = -side + hB.Y;
                        posSide = side + hB.Y;
                        negEdge = (byte)EdgeNumbers.EDGE3;
                        posEdge = (byte)EdgeNumbers.EDGE1;
                        ComputeIncidentEdge(incidentEdge, hA, posA, rotA, frontNormal);
                    }
                    break;

                case Axis.FACE_B_Y:
                    {
                        frontNormal = -normal;
                        front = (posB * frontNormal) + hB.Y;
                        sideNormal = rotB.Col1;
                        float side = (posB * sideNormal);
                        negSide = -side + hB.X;
                        posSide = side + hB.X;
                        negEdge = (byte)EdgeNumbers.EDGE2;
                        posEdge = (byte)EdgeNumbers.EDGE4;
                        ComputeIncidentEdge(incidentEdge, hA, posA, rotA, frontNormal);
                    }
                    break;
            }

            // clip other face with 5 box planes (1 face plane, 4 edge planes)

            ClipVertex[] clipPoints1 = new ClipVertex[2];
            ClipVertex[] clipPoints2 = new ClipVertex[2];
            int np;

            // Clip to box side 1
            np = ClipSegmentToLine(clipPoints1, incidentEdge, -sideNormal, negSide, negEdge);

            if (np < 2)
                return null;

            // Clip to negative box side 1
            np = ClipSegmentToLine(clipPoints2, clipPoints1, sideNormal, posSide, posEdge);

            if (np < 2)
                return null;

            // Now clipPoints2 contains the clipping points.
            // Due to roundoff, it is possible that clipping removes all points.

            //int numContacts = 0;
            List<CollisionInfo> contacts = new List<CollisionInfo>();
            for (int i = 0; i < 2; ++i)
            {
                float separation1 = (frontNormal * clipPoints2[i].v) - front;

                if (separation1 <= 0)
                {
                    var feature = clipPoints2[i].fp;
                    if (axis == Axis.FACE_B_X || axis == Axis.FACE_B_Y)
                        Flip(ref feature);

                    CollisionInfo contact = new CollisionInfo(clipPoints2[i].v - separation1 * frontNormal, normal, separation1, (byte)(feature.inEdge1 + 4 * feature.outEdge1), (byte)(feature.inEdge2 + 4 * feature.outEdge2));
                    contacts.Add(contact);
                    //contacts[numContacts].separation = separation;
                    //contacts[numContacts].normal = normal;
                    //// slide contact point onto reference face (easy to cull)
                    //contacts[numContacts].position = clipPoints2[i].v - separation * frontNormal;
                    //contacts[numContacts].feature = clipPoints2[i].fp;
                    //if (axis == FACE_B_X || axis == FACE_B_Y)
                    //    Flip(contacts[numContacts].feature);
                    //++numContacts;
                }
            }

            return contacts.ToArray();
        }
    }
}
