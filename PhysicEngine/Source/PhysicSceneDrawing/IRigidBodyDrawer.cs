using GraphicPanels;

namespace PhysicSceneDrawing
{
    //Über dieses Interface kann dem PhysicSceneDrawer von außen vorgegeben werden, wie er ein bestimmten Starrkörper zeichnen soll
    public interface IRigidBodyDrawer
    {
        void Draw(GraphicPanel2D panel);
        void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor);
    }
}
