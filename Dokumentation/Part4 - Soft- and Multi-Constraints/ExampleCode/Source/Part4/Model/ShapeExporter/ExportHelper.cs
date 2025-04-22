using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shapes;
using DynamicData;
using JsonHelper;
using Part4.Model.Editor.EditorJoint;
using Part4.Model.Editor.EditorShape;
using Part4.Model.Simulator.SimulatorJoint;
using Part4.Model.Simulator.SimulatorShape;
using PhysicEngine;
using PhysicEngine.ExportData;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.ExportData.RigidBody;

namespace Part4.Model.ShapeExporter
{
    static class ExportHelper
    {
        #region Editor
        public static string ToJson(List<IEditorShape> shapes, List<IEditorJoint> joints)
        {
            return Helper.ToCompactJson(new PhysicSceneExportData() 
            { 
                Bodies = shapes.Select(x => x.GetExportData()).ToArray(),
                Joints = joints.Select(x => x.GetExportData(shapes)).ToArray(),
            });
        }

        public static void JsonToEditorData(string json, out List<IEditorShape> shapes, out List<IEditorJoint> joints)
        {
            var rawData = Helper.FromCompactJson<PhysicSceneExportData>(json);

            shapes = new List<IEditorShape>();
            foreach (var ctor in rawData.Bodies)
            {
                shapes.Add(ExportToEditorShape(ctor));
            }

            joints = new List<IEditorJoint>();
            if (rawData.Joints != null)
            {
                foreach (var ctor in rawData.Joints)
                {
                    joints.Add(ExportToEditorJoint(ctor, shapes));
                }
            }            
        }

        public static IEditorShape ExportToEditorShape(IExportRigidBody shape)
        {
            if (shape is RectangleExportData)
                return new EditorRectangle((shape as RectangleExportData));

            if (shape is CircleExportData)
                return new EditorCircle((shape as CircleExportData));

            throw new Exception("Unknown type " + shape.GetType());
        }

        private static IEditorJoint ExportToEditorJoint(IExportJoint joint, List<IEditorShape> shapes)
        {
            if (joint is DistanceJointExportData)
                return new EditorDistanceJoint(joint as DistanceJointExportData, shapes);

            throw new Exception("Unknown type " + joint.GetType());

        }
        #endregion

        #region Simulator
        public static string ToJson(PhysicSceneExportData physicScene)
        {
            return Helper.ToCompactJson(physicScene);
        }

        public static void JsonToSimulatorData(string json, out List<ISimulatorShape> shapes, out List<ISimulatorJoint> joints)
        {
            var rawData = Helper.FromCompactJson<PhysicSceneExportData>(json);

            shapes = new List<ISimulatorShape>();
            foreach (var ctor in rawData.Bodies)
            {
                if (ctor is RectangleExportData)
                    shapes.Add(new SimulatorRectangle((ctor as RectangleExportData)));

                if (ctor is CircleExportData)
                    shapes.Add(new SimulatorCircle((ctor as CircleExportData)));
            }

            var bodies = shapes.Select(x => x.PhysicModel).ToList();

            joints = new List<ISimulatorJoint>();
            if (rawData.Joints != null)
            {
                foreach (var ctor in rawData.Joints)
                {
                    if (ctor is DistanceJointExportData)
                        joints.Add(new SimulatorDistanceJoint(ctor as DistanceJointExportData, bodies));
                }
            }
            
        }

        #endregion
    }
}
