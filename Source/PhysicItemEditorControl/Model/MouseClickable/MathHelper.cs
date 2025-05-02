using PhysicGlobal;
using System;

namespace PhysicItemEditorControl.Model.MouseClickable
{
    internal class MathHelper
    {
        //Die Linie geht von p1 nach p2
        public static bool IsPointAboveLine(Vec2D p1, Vec2D p2, Vec2D point)
        {
            Vec2D dir = (p2 - p1);
            float dirLength = dir.Length();
            if (dirLength < 0.0001f) return false;
            dir /= dirLength;
            Vec2D d = point - p1;

            float projection1 = dir * d;
            if (projection1 < 0) return false;
            if (projection1 > (p2 - p1).Length()) return false;

            float projection2 = dir.Spin90() * d;
            if (Math.Abs(projection2) > 3) return false;

            return true;
        }
    }
}
