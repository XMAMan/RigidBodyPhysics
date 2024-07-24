using KeyFrameGlobal;
using KeyFramePhysicImporter.Model.AnimationPropertys;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics.RuntimeObjects.Thruster;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace KeyFramePhysicImporter.Model
{
    //Konvertiert ein PhysicScene-Objekt in eine Liste von IAnimationProperty
    public static class PhysicSceneAnimationPropertyConverter
    {
        public static IAnimationProperty[] Convert(PhysicScenePublicData physicObjects)
        {
            List<IAnimationProperty> properties = new List<IAnimationProperty>();
            properties.AddRange(physicObjects.Joints.Select(x => JointToAnimationProperty(x)));
            properties.AddRange(physicObjects.Thrusters.Select(x => ThrusterToAnimationProperty(x)));
            return properties.ToArray();
        }

        private static IAnimationProperty JointToAnimationProperty(IPublicJoint joint)
        {
            if (joint is IPublicDistanceJoint)
            {
                var distanceJoint = (IPublicDistanceJoint)joint;
                return new FloatAnimationProperty(distanceJoint.MinLength, distanceJoint.MaxLength, () => distanceJoint.LengthPosition, (value) => distanceJoint.LengthPosition = value);
            }

            if (joint is IPublicRevoluteJoint)
            {
                var revoluteJoint = (IPublicRevoluteJoint)joint;
                return new FloatAnimationProperty(0, 1, () => revoluteJoint.MotorPosition, (value) => revoluteJoint.MotorPosition = value);
            }

            if (joint is IPublicPrismaticJoint)
            {
                var prismaticJoint = (IPublicPrismaticJoint)joint;
                return new FloatAnimationProperty(0, 1, () => prismaticJoint.MotorPosition, (value) => prismaticJoint.MotorPosition = value);
            }

            if (joint is IPublicWheelJoint)
            {
                var wheelJoint = (IPublicWheelJoint)joint;
                return new FloatAnimationProperty(0, 1, () => wheelJoint.MotorPosition, (value) => wheelJoint.MotorPosition = value);
            }

            if (joint is IPublicWeldJoint)
            {
                var weldJoint = (IPublicWeldJoint)joint;
                return new FloatAnimationProperty(0, 10, () => weldJoint.Stiffness, (value) => weldJoint.Stiffness = value);
            }

            throw new ArgumentException("Unknown type " + joint.GetType());
        }

        private static IAnimationProperty ThrusterToAnimationProperty(IPublicThruster thruster)
        {
            return new BoolAnimationProperty(() => thruster.IsEnabled, (value) => thruster.IsEnabled = value);
        }
    }
}
