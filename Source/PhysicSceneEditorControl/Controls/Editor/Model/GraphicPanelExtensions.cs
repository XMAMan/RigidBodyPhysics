using GraphicPanels;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model
{
    internal static class GraphicPanelExtensions
    {
        public static void DrawStringOnCircleBorder(this GraphicPanel2D panel, string text, float textSize, Color color, Vec2D pos, Vec2D dir)
        {
            Vec2D textPos1 = GetTextPosition(panel, text, textSize, pos, dir);
            panel.DrawString(textPos1.ToGrx(), color, textSize, text);
        }

        //Wenn ich Text neben den Rand eines Kreises schreiben will, dann gibt diese Funktion mir an, wohin ich den Text platzieren muss
        //text=Diesen Text möche ich schreiben
        //pos=Diser Punkt liegt auf den Rand eines Kreises. Neben diesen Punkt soll der Text
        //dir=Richtung vom Kreiszentrum zum Punkt pos
        private static Vec2D GetTextPosition(GraphicPanel2D panel, string text, float textSize, Vec2D pos, Vec2D dir)
        {
            var size = panel.GetStringSize(textSize, text);
            Vec2D v = new Vec2D(size.Width, size.Height) / 2;
            float radius = v.Length();
            return pos + dir * radius - v;
        }
    }
}
