using GraphicPanels;
using PhysicSceneEditorControl.Controls.Editor.Model.EditorShape;
using PhysicSceneEditorControl.Controls.Editor.Model.ShapeExporter;
using RigidBodyPhysics.MathHelper;

namespace PhysicSceneEditorControl.Controls.Editor.Model
{
    //Mit dieser Klasse habe ich erstellt, um damit die Funktionen zur Flächen und Massenberechung beim Polygon zu prüfen.
    //Ich zeichne hier eine Shape und erzeuge daraus eine Bool-2D-Map, wo ich jeden Pixel der Shape als Massepunkt der Fläche 1 betrachte.
    internal static class ShapeHelper
    {
        public static Bitmap ConvertToBitmap(IEditorShape shape)
        {
            var copy = ExportHelper.ExportToEditorShape(shape.GetExportData());

            var box = copy.GetBoundingBox();
            Vec2D localCenter = copy.Center - box.Min;
            copy.MoveTo(localCenter);

            GraphicPanel2D panel = new GraphicPanel2D() { Width = (int)box.Width, Height = (int)box.Height, Mode = Mode2D.CPU };
            panel.ClearScreen(Color.White);
            copy.BorderPen = Pens.Red;
            copy.Backcolor = Color.Red;
            copy.Draw(panel);
            panel.FlipBuffer();
            var image = panel.GetScreenShoot();
            panel.Dispose();
            return image;
        }

        private static bool[,] ShapeToBoolMap(IEditorShape shape)
        {
            Bitmap bitmap = ConvertToBitmap(shape);
            bool[,] map = new bool[bitmap.Width, bitmap.Height];
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                    map[x, y] = !CompareTwoColors(bitmap.GetPixel(x, y), Color.White);

            bitmap.Dispose();

            return map;
        }

        private static bool CompareTwoColors(Color c1, Color c2)
        {
            return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
        }

        public static float GetArea(IEditorShape shape)
        {
            bool[,] map = ShapeToBoolMap(shape);

            int counter = 0;
            for (int x = 0; x < map.GetLength(0); x++)
                for (int y = 0; y < map.GetLength(1); y++)
                    if (map[x, y]) counter++;

            return counter;
        }

        public static Vec2D CenterOfGravity(IEditorShape shape)
        {
            bool[,] map = ShapeToBoolMap(shape);
            long xSum = 0, ySum = 0;
            int counter = 0;

            for (int x = 0; x < map.GetLength(0); x++)
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y])
                    {
                        counter++;
                        xSum += x;
                        ySum += y;
                    }
                }

            return shape.GetBoundingBox().Min + new Vec2D(xSum / (float)counter, ySum / (float)counter);
        }

        //Berechnet den Inertia-Wert für eine Shape, die um ihren Schwerpunkt gedreht werden soll und wo davon ausgegangen wird, dass 
        //die Shape ein Gewicht von 1 Kg hat
        public static float GetInertia(IEditorShape shape)
        {
            var p = shape.Properties;
            float density = p.MassType1 == RigidBodyPhysics.ExportData.RigidBody.MassData.MassType.Density ? p.Density : p.Mass / GetArea(shape);

            bool[,] map = ShapeToBoolMap(shape);

            Vec2D min = shape.GetBoundingBox().Min;
            Vec2D c = CenterOfGravity(shape) - min;

            float rSum = 0;

            for (int x = 0; x < map.GetLength(0); x++)
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    if (map[x, y])
                    {
                        Vec2D r = new Vec2D(x, y) - c;
                        rSum += r * r;
                    }
                }

            return rSum * density;
        }

        public static IEditorShape GetShapeFromPoint(List<IEditorShape> shapes, Vec2D point)
        {
            return (IEditorShape)GetShapeFromPoint(shapes.Cast<ISelectableShape>().ToList(), point);
        }

        public static ISelectableShape GetShapeFromPoint(List<ISelectableShape> shapes, Vec2D point)
        {
            List<KeyValuePair<float, ISelectableShape>> list = new List<KeyValuePair<float, ISelectableShape>>();
            foreach (var shape in shapes)
            {
                shape.Backcolor = Color.Transparent;

                if (shape.IsPointInside(point))
                {
                    list.Add(new KeyValuePair<float, ISelectableShape>(shape.GetArea(), shape));
                }
            }

            if (list.Any() == false) return null;

            var selectedShape = list.OrderBy(x => x.Key).First().Value;
            selectedShape.Backcolor = Color.Red;
            return selectedShape;
        }

        public static ISelectable GetLineShapeFromPoint(List<ISelectable> selectables, Vec2D point)
        {
            var first = selectables.FirstOrDefault(x => x.IsPointInside(point));
            if (first != null)
            {
                first.Backcolor = Color.Red;
            }

            return first;
        }
    }
}
