using RigidBodyPhysics.ExportData.AxialFriction;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.ExportData.RotaryMotor;
using RigidBodyPhysics.ExportData.Thruster;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RigidBody.Polygon;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace RigidBodyPhysics.ExportData
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
                AxialFrictions = data.AxialFrictions.Select(x => x.GetExportData(bodyList)).ToArray(),
                CollisionMatrix = data.CollisionMatrix
            };
        }

        internal static PhysicSceneConstructorData FromExportData(PhysicSceneExportData data)
        {
            var bodies = BodiesFromExportData(data.Bodies);
            var joints = JointsFromExportData(data.Joints, bodies.ToList());
            var thrusters = ThrustersFromExportData(data.Thrusters, bodies);
            var motors = MotorsFromExportData(data.Motors, bodies.ToList());
            var axialFrictions = AxialFrictionsFromExportData(data.AxialFrictions, bodies.ToList());

            SetCollideExcludeListFromEachBody(bodies, joints);

            return new PhysicSceneConstructorData()
            {
                Bodies = bodies.ToArray(),
                Joints = joints.ToArray(),
                Thrusters = thrusters,
                Motors = motors,
                AxialFrictions = axialFrictions,
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
            if (exportBodies == null) return new List<IRigidBody>();
            return exportBodies.Select(BodyFromExportData).ToList();
        }

        public static IRigidBody BodyFromExportData(IExportRigidBody body)
        {
            if (body is RectangleExportData)
                return new RigidRectangle((body as RectangleExportData));

            if (body is CircleExportData)
                return new RigidCircle((body as CircleExportData));

            if (body is PolygonExportData)
            {
                var polygon = (PolygonExportData)body;
                if (polygon.PolygonType == PolygonCollisionType.EdgeWithNormalsPointingOutside || polygon.PolygonType == PolygonCollisionType.EdgeWithNormalsPointingInside)
                    return new EdgePolygon((body as PolygonExportData));

                if (polygon.PolygonType == PolygonCollisionType.Rigid)
                    return new RigidPolygon((body as PolygonExportData));
            }

            throw new NotImplementedException();
        }

        private static List<IJoint> JointsFromExportData(IExportJoint[] exportJoints, List<IRigidBody> bodies)
        {
            if (exportJoints == null) return new List<IJoint>();
            return exportJoints.Select(x => JointFromExportData(x, bodies)).ToList();
        }
        public static IJoint JointFromExportData(IExportJoint joint, List<IRigidBody> bodies)
        {
            if (joint is DistanceJointExportData)
                return new DistanceJoint(joint as DistanceJointExportData, bodies);

            if (joint is RevoluteJointExportData)
                return new RevoluteJoint(joint as RevoluteJointExportData, bodies);

            if (joint is PrismaticJointExportData)
                return new PrismaticJoint(joint as PrismaticJointExportData, bodies);

            if (joint is WeldJointExportData)
                return new WeldJoint(joint as WeldJointExportData, bodies);

            if (joint is WheelJointExportData)
                return new WheelJoint(joint as WheelJointExportData, bodies);

            throw new NotImplementedException();
        }

        private static IThruster[] ThrustersFromExportData(IExportThruster[] exportThrusters, List<IRigidBody> bodies)
        {
            if (exportThrusters == null) return new IThruster[0];

            return exportThrusters.Select(x => new RuntimeObjects.Thruster.Thruster(x as ThrusterExportData, bodies.ToList())).ToArray();
        }        

        private static IRotaryMotor[] MotorsFromExportData(IExportRotaryMotor[] exportMotors, List<IRigidBody> bodies)
        {
            if (exportMotors == null) return new IRotaryMotor[0];

            return exportMotors.Select(x => new RuntimeObjects.RotaryMotor.RotaryMotor(x as RotaryMotorExportData, bodies.ToList())).ToArray();
        }

        private static IAxialFriction[] AxialFrictionsFromExportData(IExportAxialFriction[] exportAxialFrictions, List<IRigidBody> bodies)
        {
            if (exportAxialFrictions == null) return new IAxialFriction[0];

            return exportAxialFrictions.Select(x => new RuntimeObjects.AxialFriction.AxialFriction(x as AxialFrictionExportData, bodies.ToList())).ToArray();
        }
    }
}
