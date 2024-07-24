using GraphicMinimal;
using GraphicPanels;
using System.Drawing;
using TextureEditorControl.Controls.Editor.Model.Shape;

namespace TextureEditorControl.Controls.Editor.Model
{
    //Es wurde mit der Maus der Rand oder die Ecke von ein Textur-Rechteck angeklickt
    //Dieser Klasse merkt sich, welcher Punkt des Textur-Rechtecks angeklickt wurde
    class TextureClickPoint
    {
        public IShape Shape { get; private set; }
        public RectanglePart Part { get; private set; }

        public Vector2D ClickPosition { get; private set; } //Hier erfolgte das MouseDown-Event
        public Vector2D[] Normals { get => Shape.GetNormalsFromTextureBorderPoint(Part, ClickPosition); }

        public TextureClickPoint(IShape shape, RectanglePart part, Vector2D clickPosition)
        {
            this.Shape = shape;
            this.Part = part;
            this.ClickPosition = clickPosition;
        }

        //Beim verschieben des Textur-Clickpoints wird das Textur-Rechteck verändert
        public void MoveClickPointToPosition(Vector2D position, bool rotateCornerPoint)
        {
            Vector2D xyDistance = this.Shape.GetDistanceToTextureBorderPart(this.Part, position);

            if (this.Part == RectanglePart.Center)
            {
                this.Shape.Propertys.DeltaX += (int)(xyDistance.X);
                this.Shape.Propertys.DeltaY += (int)(xyDistance.Y);
            }
            else
            {
                if (rotateCornerPoint && IsCorner(this.Part))
                {
                    //Rotate
                    float angleDiff = this.Shape.GetAngleDistanceToTextureCorner(this.Part, position);
                    this.Shape.Propertys.DeltaAngle += angleDiff;

                    if (this.Shape.Propertys.DeltaAngle > 180)
                        this.Shape.Propertys.DeltaAngle -= 360;

                    if (this.Shape.Propertys.DeltaAngle < -180)
                        this.Shape.Propertys.DeltaAngle += 360;
                }
                else
                {
                    //Change Width/Height
                    this.Shape.Propertys.Width -= (int)(xyDistance.X / 2);
                    this.Shape.Propertys.Height -= (int)(xyDistance.Y / 2);
                }
            }
        }

        private static bool IsCorner(RectanglePart part)
        {
            return
                part == RectanglePart.LeftTopCorner ||
                part == RectanglePart.RightTopCorner ||
                part == RectanglePart.LeftBottomCorner ||
                part == RectanglePart.RightBottomCorner;
        }

        public void DrawClickPoint(GraphicPanel2D panel, Vector2D point, bool rotateCornerPoint)
        {
            foreach (var normal in this.Normals)
            {
                //panel.DrawLine(new Pen(Color.Red, 5), point, point + normal * 20);
                DrawArrow(panel, point, point + normal * 20);
            }

            if (IsCorner(this.Part) && rotateCornerPoint == false)
            {
                foreach (var normal in this.Normals)
                {
                    //panel.DrawLine(new Pen(Color.Red, 5), point, point - normal * 20);
                    DrawArrow(panel, point, point - normal * 20);
                }

            }
        }

        private void DrawArrow(GraphicPanel2D panel, Vector2D p1, Vector2D p2)
        {
            Pen pen = new Pen(Color.Black, 3);
            panel.DrawLine(pen, p1, p2);
            var v1 = Vector2D.GetV2FromAngle360(p2 - p1, 135) / 2;
            var v2 = Vector2D.GetV2FromAngle360(p2 - p1, -135) / 2;
            panel.DrawLine(pen, p2, p2 + v1);
            panel.DrawLine(pen, p2, p2 + v2);
        }
    }
}
