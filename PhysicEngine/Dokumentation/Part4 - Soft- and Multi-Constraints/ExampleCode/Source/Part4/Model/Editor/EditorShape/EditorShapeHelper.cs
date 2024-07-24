using GraphicMinimal;
using PhysicEngine.MathHelper;
using System;

namespace Part4.Model.Editor.EditorShape
{
    internal static class EditorShapeHelper
    {
        //Gibt im lokalen Body-Space den Richtungsvektro r von body.Center nach position zurück
        public static Vector2D GetLocalBodyDirection(IEditorShape shape, Vector2D position)
        {
            Matrix2x2 toLocal = Matrix2x2.Rotate(-shape.AngleInDegree / 180 * (float)Math.PI);
            return toLocal * (position - shape.Center);
        }

        public static Vector2D LocalBodyDirectionToWorldPosition(IEditorShape shape, Vector2D localBodyDirection)
        {
            Matrix2x2 toWorld = Matrix2x2.Rotate(shape.AngleInDegree / 180 * (float)Math.PI);
            return shape.Center + toWorld * localBodyDirection;
        }
    }
}
