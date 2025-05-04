using PhysicGlobal;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model.EditorShape
{
    internal static class EditorShapeHelper
    {
        //Gibt im lokalen Body-Space den Richtungsvektro r von body.Center nach position zurück
        public static Vec2D GetLocalBodyDirectionFromPosition(IEditorShape shape, Vec2D position)
        {
            Matrix2x2 toLocal = Matrix2x2.Rotate(-shape.AngleInDegree / 180 * (float)Math.PI);
            return toLocal * (position - shape.Center);
        }

        public static Vec2D GetLocalBodyDirectionFromWorldDirection(IEditorShape shape, Vec2D worldDirection)
        {
            Matrix2x2 toLocal = Matrix2x2.Rotate(-shape.AngleInDegree / 180 * (float)Math.PI);
            return toLocal * worldDirection;
        }

        public static Vec2D LocalBodyDirectionToWorldPosition(IEditorShape shape, Vec2D localBodyDirection)
        {
            Matrix2x2 toWorld = Matrix2x2.Rotate(shape.AngleInDegree / 180 * (float)Math.PI);
            return shape.Center + toWorld * localBodyDirection;
        }

        public static Vec2D LocalBodyDirectionToWorldDirection(IEditorShape shape, Vec2D localBodyDirection)
        {
            Matrix2x2 toWorld = Matrix2x2.Rotate(shape.AngleInDegree / 180 * (float)Math.PI);
            return toWorld * localBodyDirection;
        }

        //Die Linie geht von p1 nach p2
        public static bool IsPointAboveLine(Vec2D p1, Vec2D p2, Vec2D point)
        {
            return PhysicGlobal.MathHelper.IsPointAboveLine(p1, p2, point, 3);
        }
    }
}
