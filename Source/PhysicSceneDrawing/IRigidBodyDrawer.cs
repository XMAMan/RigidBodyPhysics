using PhysicGlobal;

namespace PhysicSceneDrawing
{
    //Über dieses Interface kann dem PhysicSceneDrawer von außen vorgegeben werden, wie er ein bestimmten Starrkörper zeichnen soll
    public interface IRigidBodyDrawer
    {
        void Draw(IDrawingPanel panel);
        void DrawWithTwoColors(IDrawingPanel panel, Color frontColor, Color backColor);
    }
}
