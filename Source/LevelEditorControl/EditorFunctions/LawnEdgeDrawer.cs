using GraphicPanels;
using LevelEditorControl.LevelItems.Polygon;
using LevelEditorExports.Editor.Helper;
using PhysicGlobal;
using System.Drawing;
using System.Linq;
using WpfControls.Extensions;

namespace LevelEditorControl.EditorFunctions
{
    //Zeichnet eine Rasenkante um ein Polygon
    internal class LawnEdgeDrawer : LawnEdgePositionCalculator
    {
        public string TextureFile = "#00FF00";
       
        public float ZValue = 0;

        public LawnEdgeDrawer(ILevelItemPolygon polygon)
            : base(polygon)
        {
        }

        public void DrawLawn(PolygonPoint p1, PolygonPoint p2, GraphicPanel2D panel)
        {
            var segments = GetAllSegments(p1, p2);

            string texture = this.TextureFile;
            float height = this.LawnHeight;
            panel.ZValue2D = this.ZValue;

            foreach (var segment in segments)
            {
                Vec2D center = segment.GetCenter();
                float width = segment.GetWidth();
                float angle = segment.GetAngle();

                if (string.IsNullOrEmpty(texture) == false)
                    panel.DrawFillRectangle(texture, (int)center.X, (int)center.Y, (int)width, (int)height, true, Color.White, angle);
                else
                    panel.DrawPolygon(new Pen(Color.Green, 2), segment.Points.ToGrx().ToList());

            }
        }

    }
}
