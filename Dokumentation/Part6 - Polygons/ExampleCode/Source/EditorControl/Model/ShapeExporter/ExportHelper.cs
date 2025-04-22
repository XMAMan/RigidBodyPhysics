using EditorControl.Model.EditorJoint;
using EditorControl.Model.EditorShape;
using PhysicEngine.ExportData.Joints;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.ExportData;
using JsonHelper;
using EditorControl.Model.Function;
using PhysicEngine.ExportData.Thruster;
using PhysicEngine.ExportData.RotaryMotor;

namespace EditorControl.Model.ShapeExporter
{
    static internal class ExportHelper
    {
        #region Editor
        public static string ToJson(FunctionData functionData)
        {
            return Helper.ToCompactJson(ConvertFunctionDataToExportData(functionData));
        }

        public static PhysicSceneExportData ConvertFunctionDataToExportData(FunctionData functionData)
        {
            return new PhysicSceneExportData()
            {
                Bodies = functionData.Shapes.Select(x => x.GetExportData()).ToArray(),
                Joints = functionData.Joints.Select(x => x.GetExportData(functionData.Shapes)).ToArray(),
                Thrusters = functionData.Thrusters.Select(x => x.GetExportData(functionData.Shapes)).ToArray(),
                Motors = functionData.RotaryMotors.Select(x => x.GetExportData(functionData.Shapes)).ToArray(),
                CollisionMatrix = functionData.CollisionMatrix
            };
        }

        public static FunctionData JsonToEditorData(string json)
        {
            var rawData = Helper.FromCompactJson<PhysicSceneExportData>(json);

            return ConvertExportDataToFunctionData(rawData);
        }

        public static FunctionData ConvertExportDataToFunctionData(PhysicSceneExportData rawData)
        {
            var functionData = new FunctionData();

            foreach (var ctor in rawData.Bodies)
            {
                functionData.Shapes.Add(ExportToEditorShape(ctor));
            }

            if (rawData.Joints != null)
            {
                foreach (var ctor in rawData.Joints)
                {
                    functionData.Joints.Add(ExportToEditorJoint(ctor, functionData.Shapes));
                }
            }

            if (rawData.Thrusters != null)
            {
                foreach (var ctor in rawData.Thrusters)
                {
                    functionData.Thrusters.Add(new EditorThruster.EditorThruster(ctor as ThrusterExportData, functionData.Shapes));
                }
            }

            if (rawData.Motors != null)
            {
                foreach (var ctor in rawData.Motors)
                {
                    functionData.RotaryMotors.Add(new EditorRotaryMotor.EditorRotaryMotor(ctor as RotaryMotorExportData, functionData.Shapes));
                }
            }

            if (rawData.CollisionMatrix != null)
                functionData.CollisionMatrix = rawData.CollisionMatrix;

            return functionData;
        }

        public static IEditorShape ExportToEditorShape(IExportRigidBody shape)
        {
            if (shape is RectangleExportData)
                return new EditorRectangle((shape as RectangleExportData));

            if (shape is CircleExportData)
                return new EditorCircle((shape as CircleExportData));

            if (shape is PolygonExportData)
                return new EditorPolygon(shape as PolygonExportData);

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
