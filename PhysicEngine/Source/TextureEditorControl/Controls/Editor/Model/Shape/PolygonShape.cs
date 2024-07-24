using GraphicMinimal;
using GraphicPanels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TextureEditorGlobal;
using WpfControls.Controls.CameraSetting;

namespace TextureEditorControl.Controls.Editor.Model.Shape
{
    class PolygonShape : AreaShape, IShape
    {
        public PolygonShape(I2DAreaShape polygon)
            : base(polygon)
        {
            this.BoundingBox = GetBoundingBox();
        }

        protected override Vector2D[] GetPhysicCornerPoints()
        {
            return (this.shape as IPolygon).Points;
        }

        protected override void DrawTexture(GraphicPanel2D panel, Camera2D camera)
        {
            if (string.IsNullOrEmpty(this.Propertys.TextureFile) == false)
            {
                var r = (IPolygon)this.shape;
                var p = this.Propertys;

                int col = Math.Min(255, Math.Max(0, (int)(255 * p.ColorFactor)));

                var texPoints = GetTextureBorderPoints()
                    .Select(x => camera.PointToScreen(x.ToPointF()).ToGrx())
                    .ToList();

                float xLength = (texPoints[1] - texPoints[0]).Length();
                float yLength = (texPoints[3] - texPoints[0]).Length();
                var xDir = (texPoints[1] - texPoints[0]) / xLength;
                var yDir = (texPoints[3] - texPoints[0]) / yLength;

                List<Vertex2D> points = new List<Vertex2D>();
                foreach (var local in r.Points)
                {
                    var screen = camera.PointToScreen(local.ToPointF()).ToGrx();
                    var tex = new Vector2D((screen - texPoints[0]) * xDir / xLength, (screen - texPoints[0]) * yDir / yLength);
                    points.Add(new Vertex2D(screen, tex));
                }

                panel.DrawFillPolygon(p.TextureFile, p.MakeFirstPixelTransparent, Color.FromArgb(col, col, col), points);
            }
        }

        public override bool IsPointInPhysicModel(Vector2D point)
        {
            var p = (IPolygon)this.shape;
            return MathHelper.PointIsInsidePolygon(p.Points, point);
        }
    }
}
