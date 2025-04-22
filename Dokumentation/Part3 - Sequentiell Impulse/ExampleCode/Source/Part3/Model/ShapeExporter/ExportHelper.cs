using System;
using System.Collections.Generic;
using System.Linq;
using JsonHelper;
using Part3.Model.Editor.EditorShape;
using Part3.Model.Simulator.SimulatorShape;
using PhysicEngine.ExportData;

namespace Part3.Model.ShapeExporter
{
    static class ExportHelper
    {
        #region Editor
        public static string ToJson(List<IEditorShape> shapes)
        {
            return Helper.ToCompactJson(shapes.Select(x => x.GetExportData()).ToArray());
        }

        public static List<IEditorShape> JsonToEditorShape(string json)
        {
            var rawData = Helper.FromCompactJson<IExportShape[]>(json);

            List<IEditorShape> shapes = new List<IEditorShape>();
            foreach (var ctor in rawData)
            {
                shapes.Add(ExportToEditorShape(ctor));
            }

            return shapes;
        }

        public static IEditorShape ExportToEditorShape(IExportShape shape)
        {
            if (shape is RectangleExportData)
                return new EditorRectangle((shape as RectangleExportData));

            if (shape is CircleExportData)
                return new EditorCircle((shape as CircleExportData));

            throw new Exception("Unknown type " + shape.GetType());
        }
        #endregion

        #region Simulator
        public static string ToJson(List<ISimulatorShape> shapes)
        {
            return Helper.ToCompactJson(shapes.Select(x => x.GetConstructorData()).ToArray());
        }

        public static List<ISimulatorShape> JsonToSimulatorShape(string json)
        {
            var rawData = Helper.FromCompactJson<IExportShape[]>(json);

            List<ISimulatorShape> shapes = new List<ISimulatorShape>();
            foreach (var ctor in rawData)
            {
                if (ctor is RectangleExportData)
                    shapes.Add(new SimulatorRectangle((ctor as RectangleExportData)));

                if (ctor is CircleExportData)
                    shapes.Add(new SimulatorCircle((ctor as CircleExportData)));
            }

            return shapes;
        }
        #endregion
    }
}
