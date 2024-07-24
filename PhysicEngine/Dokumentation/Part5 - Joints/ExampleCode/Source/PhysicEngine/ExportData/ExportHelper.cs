using PhysicEngine.ExportData.Joints;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.Joints;
using PhysicEngine.RigidBody;

namespace PhysicEngine.ExportData
{
    internal static class ExportHelper
    {
        internal static PhysicSceneExportData ToExportData(PhysicSceneConstructorData data)
        {
            var bodyList = data.Bodies.ToList();
            return new PhysicSceneExportData()
            {
                Bodies = data.Bodies.Select(x => x.GetExportData()).ToArray(),
                Joints = data.Joints.Select(x => x.GetExportData(bodyList)).ToArray()
            };
        }

        internal static PhysicSceneConstructorData FromExportData(PhysicSceneExportData data)
        {
            var bodies = BodiesFromExportData(data.Bodies);
            var joints = JointsFromExportData(data.Joints, bodies.ToList());

            SetCollideExcludeListFromEachBody(bodies, joints);

            return new PhysicSceneConstructorData()
            {
                Bodies = bodies.ToArray(),
                Joints = joints.ToArray()
            };
        }

        private static void SetCollideExcludeListFromEachBody(List<IRigidBody> bodies, List<IJoint> joints)
        {
            //Initial clear
            bodies.ForEach(x => x.CollideExcludeList.Clear());  

            //Add for each Joint
            foreach (var joint in joints)
            {
                if (joint.CollideConnected == false)
                {
                    joint.B1.CollideExcludeList.Add(joint.B2);
                    joint.B2.CollideExcludeList.Add(joint.B1);
                }
            }

            //Remove doubles
            foreach (var body in bodies)
            {
                var distinct = body.CollideExcludeList.Distinct().ToList();
                body.CollideExcludeList.Clear();
                body.CollideExcludeList.AddRange(distinct);
            }
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
                    result.Add(new DistanceJoint(joint as DistanceJointExportData, bodies));

                if (joint is RevoluteJointExportData)
                    result.Add(new RevoluteJoint(joint as RevoluteJointExportData, bodies));

                if (joint is PrismaticJointExportData)
                    result.Add(new PrismaticJoint(joint as PrismaticJointExportData, bodies));

                if (joint is WeldJointExportData)
                    result.Add(new WeldJoint(joint as WeldJointExportData, bodies));

                if (joint is WheelJointExportData)
                    result.Add(new WheelJoint(joint as WheelJointExportData, bodies));
            }

            return result;
        }
    }
}
