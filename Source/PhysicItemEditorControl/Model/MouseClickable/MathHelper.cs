using PhysicGlobal;

namespace PhysicItemEditorControl.Model.MouseClickable
{
    internal class MathHelper
    {
        //Die Linie geht von p1 nach p2
        public static bool IsPointAboveLine(Vec2D p1, Vec2D p2, Vec2D point)
        {
            return PhysicGlobal.MathHelper.IsPointAboveLine(p1, p2, point, 3);
        }
    }
}
