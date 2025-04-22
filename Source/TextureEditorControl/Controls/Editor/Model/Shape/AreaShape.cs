using GraphicMinimal;
using GraphicPanels;
using Splat;
using System;
using System.Drawing;
using System.Linq;
using TextureEditorControl.Controls.DrawingSettings;
using TextureEditorControl.Controls.TextureData;
using TextureEditorGlobal;
using WpfControls.Controls.CameraSetting;

namespace TextureEditorControl.Controls.Editor.Model.Shape
{
    //Ein Objekt, was ein Zentrum und eine Ausrichtung hat. Um dieses Objekt wird eine gedrehte BoundingBox platziert, welche das
    //Texture-Rechteck darstellt.
    abstract class AreaShape : IShape
    {
        protected I2DAreaShape shape;

        public TextureDataViewModel Propertys { get; protected set; }
        public RectangleF BoundingBox { get; protected set; }
        public bool IsSelected { get; set; } = false;

        public AreaShape(I2DAreaShape rectangle)
        {
            this.shape = rectangle;

            this.Propertys = new TextureDataViewModel()
            {
                Width = (int)rectangle.LocalBoundingBox.Width,
                Height = (int)rectangle.LocalBoundingBox.Height,
            };
        }

        protected RectangleF GetBoundingBox()
        {
            Vector2D[] points = GetPhysicCornerPoints();
            var min = new Vector2D(points.Min(x => x.X), points.Min(x => x.Y));
            var max = new Vector2D(points.Max(x => x.X), points.Max(x => x.Y));

            return new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        protected abstract Vector2D[] GetPhysicCornerPoints();

        //Die ersten 4 Punkte sind die Eckpuntke vom Textur-Rechteck. Punkt 5 ist dessen Zentrum.
        protected Vector2D[] GetTextureBorderPoints()
        {
            var r = this.shape;
            var p = this.Propertys;

            return TextureRectangleHelper.GetTextureBorderPoints(r.Center, r.LocalBoundingBox.Center().ToGrx(), p.Width, p.Height, r.AngleInDegree, p.DeltaX, p.DeltaY, p.DeltaAngle);
        }

        public void Draw(GraphicPanel2D panel, Camera2D camera, DrawingSettingsViewModel settings)
        {
            //Physik-Model-Rechteck
            if (settings.ShowPhysikModel)
                DrawPhysicModel(panel, camera);

            //Textur    
            if (settings.ShowTexture)
                DrawTexture(panel, camera);

            //Textur-Rahmen
            if (settings.ShowTextureBorder)
                DrawTextureBorder(panel, camera);
        }

        virtual protected void DrawPhysicModel(GraphicPanel2D panel, Camera2D camera)
        {
            var cornerPoints = GetPhysicCornerPoints();
            var points = cornerPoints.Select(x => camera.PointToScreen(x.ToPointF()).ToGrx()).ToList();

            panel.DrawPolygon(this.IsSelected ? new Pen(Color.Red, 4) : Pens.Black, points);
        }

        virtual protected void DrawTexture(GraphicPanel2D panel, Camera2D camera)
        {
            if (string.IsNullOrEmpty(this.Propertys.TextureFile) == false)
            {
                var r = this.shape;
                var p = this.Propertys;

                var texPoints = GetTextureBorderPoints()
                    .Select(x => camera.PointToScreen(x.ToPointF()).ToGrx())
                    .ToList();

                int col = Math.Min(255, Math.Max(0, (int)(255 * p.ColorFactor)));
                float angleInDegree = r.AngleInDegree + p.DeltaAngle;

                var texPointsCenter = 0.5f * (texPoints[0] + texPoints[2]);

                panel.DrawFillRectangle(p.TextureFile,
                       (int)texPointsCenter.X, (int)texPointsCenter.Y,
                       (int)camera.LengthToScreen(p.Width), (int)camera.LengthToScreen(p.Height),
                       p.MakeFirstPixelTransparent,
                       Color.FromArgb(col, col, col),
                       angleInDegree);
            }
        }

        protected void DrawTextureBorder(GraphicPanel2D panel, Camera2D camera)
        {
            var texPoints = GetTextureBorderPoints()
                    .Select(x => camera.PointToScreen(x.ToPointF()).ToGrx())
                    .ToList();

            panel.DrawPolygon(Pens.Green, texPoints.Take(4).ToList());

            if (this.IsSelected)
            {
                for (int i = 0; i < texPoints.Count; i++)
                {
                    panel.DrawFillCircle(Color.Green, texPoints[i], 3);
                }
            }
        }

        public abstract bool IsPointInPhysicModel(Vector2D point);

        public bool IsPointInTextureBorder(Vector2D point)
        {
            return MathHelper.IsPointInRectangle(GetTextureBorderPoints().Take(4).ToArray(), point);
        }

        public RectanglePart GetSelectedPartFromTextureBorder(Vector2D point)
        {
            var texPoints = GetTextureBorderPoints();

            float pointDistance = 10;
            float lineWidth = 3;

            if ((texPoints[0] - point).Length() < pointDistance) return RectanglePart.LeftTopCorner;
            if ((texPoints[1] - point).Length() < pointDistance) return RectanglePart.RightTopCorner;
            if ((texPoints[2] - point).Length() < pointDistance) return RectanglePart.RightBottomCorner;
            if ((texPoints[3] - point).Length() < pointDistance) return RectanglePart.LeftBottomCorner;
            if (MathHelper.IsPointAboveLine(texPoints[0], texPoints[1], point, lineWidth)) return RectanglePart.TopEdge;
            if (MathHelper.IsPointAboveLine(texPoints[1], texPoints[2], point, lineWidth)) return RectanglePart.RightEdge;
            if (MathHelper.IsPointAboveLine(texPoints[2], texPoints[3], point, lineWidth)) return RectanglePart.BottomEdge;
            if (MathHelper.IsPointAboveLine(texPoints[3], texPoints[0], point, lineWidth)) return RectanglePart.LeftEdge;
            if ((texPoints[4] - point).Length() < pointDistance) return RectanglePart.Center;

            return RectanglePart.None;
        }

        public Vector2D[] GetNormalsFromTextureBorderPoint(RectanglePart part, Vector2D point)
        {
            var texPoints = GetTextureBorderPoints();

            Vector2D n1 = (texPoints[1] - texPoints[0]).Normalize().Spin90(); //TopEdge
            Vector2D n2 = (texPoints[2] - texPoints[1]).Normalize().Spin90(); //RightEdge
            Vector2D n3 = (texPoints[3] - texPoints[2]).Normalize().Spin90(); //BottomEdge
            Vector2D n4 = (texPoints[0] - texPoints[3]).Normalize().Spin90(); //LeftEdge

            switch (part)
            {
                case RectanglePart.LeftTopCorner:
                    return new Vector2D[] { n4, n1 };

                case RectanglePart.RightTopCorner:
                    return new Vector2D[] { n2, n1 };

                case RectanglePart.RightBottomCorner:
                    return new Vector2D[] { n2, n3 };

                case RectanglePart.LeftBottomCorner:
                    return new Vector2D[] { n4, n3 };

                case RectanglePart.TopEdge:
                    return new Vector2D[] { n1, -n1 };

                case RectanglePart.RightEdge:
                    return new Vector2D[] { n2, -n2 };

                case RectanglePart.BottomEdge:
                    return new Vector2D[] { n3, -n3 };

                case RectanglePart.LeftEdge:
                    return new Vector2D[] { n4, -n4 };

                case RectanglePart.Center:
                    return new Vector2D[] { n1, n2, -n1, -n2 };
            }

            throw new ArgumentException(part.ToString());
        }

        public Vector2D GetDistanceToTextureBorderPart(RectanglePart part, Vector2D point)
        {
            var texPoints = GetTextureBorderPoints();

            switch (part)
            {
                case RectanglePart.LeftTopCorner:
                    return new Vector2D(
                        MathHelper.GetNormalDistanceToLine(texPoints[3], texPoints[0], point), //X-Abstand zu LeftEdge
                        MathHelper.GetNormalDistanceToLine(texPoints[0], texPoints[1], point));//Y-Abstand zu TopEdge

                case RectanglePart.RightTopCorner:
                    return new Vector2D(
                        MathHelper.GetNormalDistanceToLine(texPoints[1], texPoints[2], point), //X-Abstand zu RightEdge
                        MathHelper.GetNormalDistanceToLine(texPoints[0], texPoints[1], point));//Y-Abstand zu TopEdge

                case RectanglePart.RightBottomCorner:
                    return new Vector2D(
                        MathHelper.GetNormalDistanceToLine(texPoints[1], texPoints[2], point), //X-Abstand zu RightEdge
                        MathHelper.GetNormalDistanceToLine(texPoints[2], texPoints[3], point));//Y-Abstand zu BottomEdge

                case RectanglePart.LeftBottomCorner:
                    return new Vector2D(
                        MathHelper.GetNormalDistanceToLine(texPoints[3], texPoints[0], point), //X-Abstand zu LeftEdge
                        MathHelper.GetNormalDistanceToLine(texPoints[2], texPoints[3], point));//Y-Abstand zu BottomEdge

                case RectanglePart.TopEdge:
                    return new Vector2D(0, MathHelper.GetNormalDistanceToLine(texPoints[0], texPoints[1], point)); //Y-Abstand zu TopEdge

                case RectanglePart.RightEdge:
                    return new Vector2D(MathHelper.GetNormalDistanceToLine(texPoints[1], texPoints[2], point), 0); //X-Abstand zu RightEdge

                case RectanglePart.BottomEdge:
                    return new Vector2D(0, MathHelper.GetNormalDistanceToLine(texPoints[2], texPoints[3], point)); //Y-Abstand zu BottomEdge

                case RectanglePart.LeftEdge:
                    return new Vector2D(MathHelper.GetNormalDistanceToLine(texPoints[3], texPoints[0], point), 0); //X-Abstand zu LeftEdge

                case RectanglePart.Center:
                    return point - texPoints[4];
            }

            throw new ArgumentException("Distance can not be calculated for part " + part);
        }

        public float GetAngleDistanceToTextureCorner(RectanglePart part, Vector2D point)
        {
            var texPoints = GetTextureBorderPoints();

            switch (part)
            {
                case RectanglePart.LeftTopCorner:
                    return Vector2D.Angle360(texPoints[0] - texPoints[4], point - texPoints[4]);

                case RectanglePart.RightTopCorner:
                    return Vector2D.Angle360(texPoints[1] - texPoints[4], point - texPoints[4]);

                case RectanglePart.RightBottomCorner:
                    return Vector2D.Angle360(texPoints[2] - texPoints[4], point - texPoints[4]);

                case RectanglePart.LeftBottomCorner:
                    return Vector2D.Angle360(texPoints[3] - texPoints[4], point - texPoints[4]);
            }

            throw new ArgumentException("Distance can not be calculated for part " + part);
        }
    }
}
