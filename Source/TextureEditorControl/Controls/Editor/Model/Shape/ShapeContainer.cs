using DynamicData;
using GraphicMinimal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TextureEditorControl.Controls.DrawingSettings;
using TextureEditorGlobal;

namespace TextureEditorControl.Controls.Editor.Model.Shape
{
    class ShapeContainer
    {
        public IShape[] Shapes { get; private set; }
        private IShape selectedShape = null;

        public RectangleF BoundingBox { get; }

        public ShapeContainer(VisualisizerInputData data)
        {
            this.Shapes = data.Shapes.Select(Convert).ToArray();
            this.BoundingBox = GetBoundingBox(this.Shapes.Select(x => x.BoundingBox));
        }

        private static RectangleF GetBoundingBox(IEnumerable<RectangleF> boxes)
        {
            var min = new PointF(boxes.Min(x => x.X), boxes.Min(y => y.Y));
            var max = new PointF(boxes.Max(x => x.X + x.Width), boxes.Max(y => y.Y + y.Height));

            return new RectangleF(min, new SizeF(max.X - min.X, max.Y - min.Y));
        }

        private static IShape Convert(I2DAreaShape shape)
        {
            if (shape is IRectangle)
                return new RectangleShape((IRectangle)shape);

            if (shape is IPolygon)
                return new PolygonShape((IPolygon)shape);

            if (shape is ICircle)
                return new CircleShape((ICircle)shape);

            throw new ArgumentException("Unknown Type: " + shape.GetType().Name);
        }

        public IShape GetShapeFromClickPosition(Vector2D position, DrawingSettingsViewModel settings)
        {
            List<KeyValuePair<float, IShape>> clicks = new List<KeyValuePair<float, IShape>>();
            foreach (var shape in this.Shapes)
            {
                if (settings.ShowPhysikModel && shape.IsPointInPhysicModel(position) ||
                    settings.ShowTextureBorder && shape.IsPointInTextureBorder(position))
                {
                    float shapeTextureArea = shape.Propertys.Width * shape.Propertys.Height;
                    clicks.Add(new KeyValuePair<float, IShape>(shapeTextureArea, shape));
                }
            }

            if (clicks.Any() == false) return null;

            return clicks.OrderBy(x => x.Key).First().Value; //Gib die Shape mit der kleinsten Fläche zurück, wenn mehrere Shapes übereinander liegen
        }

        public void SelectShape(IShape shape)
        {
            foreach (var s in this.Shapes)
            {
                s.IsSelected = false;

                if (s == shape)
                    s.IsSelected = true;
            }

            this.selectedShape = shape;
        }

        public TextureClickPoint GetClickPointFromSelectedShape(Vector2D position)
        {
            if (this.selectedShape != null)
            {
                var part = this.selectedShape.GetSelectedPartFromTextureBorder(position);
                if (part != RectanglePart.None)
                {
                    return new TextureClickPoint(this.selectedShape, part, position);
                }
            }

            return null;
        }

        public string[] GetAllShapeNames()
        {
            return this.Shapes.Select((value, index) => index + " " + value.GetType().Name).ToArray();
        }

        public string GetShapeName(IShape shape)
        {
            return this.Shapes.IndexOf(shape) + " " + shape.GetType().Name;
        }

        public VisualisizerOutputData GetExportData()
        {
            return new VisualisizerOutputData(this.Shapes.Select(x => x.Propertys.GetExportData()).ToArray());
        }

        public void LoadExportData(VisualisizerOutputData data)
        {
            if (data.Textures.Length != this.Shapes.Length) throw new Exception("Texure-Count-Missmatch");

            for (int i = 0; i < data.Textures.Length; i++)
            {
                this.Shapes[i].Propertys.LoadExportData(data.Textures[i]);
            }
        }
    }
}
