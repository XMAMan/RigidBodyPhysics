using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.ExportData;
using JsonHelper;

namespace EditorControl.Model.ShapeExporter
{
    static internal class ExportHelper
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

            if (joint is RevoluteJointExportData)
                return new EditorRevoluteJoint(joint as RevoluteJointExportData, shapes);

            if (joint is PrismaticJointExportData)
                return new EditorPrismaticJoint(joint as PrismaticJointExportData, shapes);

            if (joint is WeldJointExportData)
                return new EditorWeldJoint(joint as WeldJointExportData, shapes);

            if (joint is WheelJointExportData)
                return new EditorWheelJoint(joint as WheelJointExportData, shapes);

            throw new Exception("Unknown type " + joint.GetType());

        }
        #endregion
    }
}
