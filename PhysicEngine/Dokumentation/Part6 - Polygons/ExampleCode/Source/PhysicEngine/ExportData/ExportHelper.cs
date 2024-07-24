using PhysicEngine.ExportData.Joints;
using PhysicEngine.ExportData.RigidBody;
using PhysicEngine.ExportData.RotaryMotor;
using PhysicEngine.ExportData.Thruster;
using PhysicEngine.Joints;
using PhysicEngine.RigidBody;
using PhysicEngine.RigidBody.Polygon;
using PhysicEngine.RotaryMotor;
using PhysicEngine.Thruster;

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
                Joints = data.Joints.Select(x => x.GetExportData(bodyList)).ToArray(),
                Thrusters = data.Thrusters.Select(x => x.GetExportData(bodyList)).ToArray(),
                Motors = data.Motors.Select(x => x.GetExportData(bodyList)).ToArray(),
                CollisionMatrix = data.CollisionMatrix
            };
        }

        internal static PhysicSceneConstructorData FromExportData(PhysicSceneExportData data)
        {
            var bodies = BodiesFromExportData(data.Bodies);
            var joints = JointsFromExportData(data.Joints, bodies.ToList());
            var thrusters = ThrustersFromExportData(data.Thrusters, bodies);
            var motors = MotorsFromExportData(data.Motors, bodies.ToList());

            SetCollideExcludeListFromEachBody(bodies, joints);

            return new PhysicSceneConstructorData()
            {
                Bodies = bodies.ToArray(),
                Joints = joints.ToArray(),
                Thrusters = thrusters,
                Motors = motors,
                CollisionMatrix = data.CollisionMatrix != null ? data.CollisionMatrix : new bool[1, 1] { { true } }
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

                if (shape is PolygonExportData)
                {
                    var polygon = (PolygonExportData)shape;
                    if (polygon.PolygonType == PolygonCollisionType.EdgeWithNormalsPointingOutside || polygon.PolygonType == PolygonCollisionType.EdgeWithNormalsPointingInside)
                        result.Add(new EdgePolygon((shape as PolygonExportData)));

                    if (polygon.PolygonType == PolygonCollisionType.Rigid)
                        result.Add(new RigidPolygon((shape as PolygonExportData)));
                }
                    
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

        private static IThruster[] ThrustersFromExportData(IExportThruster[] exportThrusters, List<IRigidBody> bodies)
        {
            if (exportThrusters == null) return new IThruster[0];

            return exportThrusters.Select(x => new PhysicEngine.Thruster.Thruster(x as ThrusterExportData, bodies.ToList())).ToArray();
        }

        private static IRotaryMotor[] MotorsFromExportData(IExportRotaryMotor[] exportMotors, List<IRigidBody> bodies)
        {
            if (exportMotors == null) return new IRotaryMotor[0];

            return exportMotors.Select(x => new PhysicEngine.RotaryMotor.RotaryMotor(x as RotaryMotorExportData, bodies.ToList())).ToArray();
        }
    }
}
