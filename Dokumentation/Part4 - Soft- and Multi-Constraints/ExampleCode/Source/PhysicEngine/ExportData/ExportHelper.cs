using PhysicEngine.ExportData.Joints;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.Joints;
using PhysicEngine.RigidBody;

namespace PhysicEngine.ExportData
{
    public static class ExportHelper
    {
        public static PhysicSceneExportData ToExportData(PhysicSceneConstructorData data)
        {
            var bodyList = data.Bodies.ToList();
            return new PhysicSceneExportData()
            {
                Bodies = data.Bodies.Select(x => x.GetExportData()).ToArray(),
                Joints = data.Joints.Select(x => x.GetExportData(bodyList)).ToArray()
            };
        }

        public static PhysicSceneConstructorData FromExportData(PhysicSceneExportData data)
        {
            var bodies = BodiesFromExportData(data.Bodies);
            var joints = JointsFromExportData(data.Joints, bodies.ToList());
            return new PhysicSceneConstructorData()
            {
                Bodies = bodies.ToArray(),
                Joints = joints.ToArray()
            };
        }

        private static List<IRigidBody> BodiesFromExportData(IExportRigidBody[] exportBodies)
        {
            List<IRigidBody> result = new List<IRigidBody>();

            foreach (var shape in exportBodies)
            {
                if (shape is RectangleExportData)
                    result.Add(new RigidRectangle((shape as RectangleExportData)));

                if (shape is CircleExportData)
                    result.Add(new RigidCircle((shape as CircleExportData)));
            }

            return result;
        }

        private static List<IJoint> JointsFromExportData(IExportJoint[] exportJoints, List<IRigidBody> bodies)
        {
            if (exportJoints == null) return new List<IJoint>();

            List<IJoint> result = new List<IJoint>();

            foreach (var joint in exportJoints)
            {
                if (joint is DistanceJointExportData)
                    result.Add(new DistanceJoint((joint as DistanceJointExportData), bodies));
            }

            return result;
        }
    }
}
