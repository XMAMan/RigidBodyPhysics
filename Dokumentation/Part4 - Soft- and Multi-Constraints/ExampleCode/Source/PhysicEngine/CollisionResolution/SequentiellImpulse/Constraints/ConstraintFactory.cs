using PhysicEngine.Joints;
using PhysicEngine.MouseBodyClick;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse.Constraints
{
    internal class ConstraintFactory
    {
        public IConstraint[] CreateConstraints(ConstraintConstructorData data, CollisionPointWithImpulse[] collisions)
        {
            List<IConstraint> constraints = new List<IConstraint>();

            constraints.AddRange(collisions.Select(x => new NormalConstraint(data, x)));
            constraints.AddRange(collisions.Select(x => new FrictionConstraint(data, x)));
            
            constraints.AddRange(data.Joints.SelectMany(x => JointToConstraint(data, x)));
 
            if (data.MouseData != null)
            {
                constraints.Add(CreateMouseConstraint(data, data.MouseData));
            }

            return constraints.ToArray();
        }

        private static MouseConstraint CreateMouseConstraint(ConstraintConstructorData data, MouseConstraintData mouseData)
        {
            return new MouseConstraint(data, mouseData);
        }

        private static List<IConstraint> JointToConstraint(ConstraintConstructorData data, IJoint joint)
        {
            if (joint is DistanceJoint)
            {
                var j = joint as DistanceJoint;
                List<IConstraint> list = new List<IConstraint>();
                list.Add(new DistanceJointConstraint(data, j));
                if (j.ParameterType != ExportData.Joints.DistanceJointExportData.SpringParameter.NoSoftness && ( j.MinLength > 0 || j.MaxLength < float.MaxValue))
                    list.Add(new MinMaxDistanceConstraint(data, j));
                return list;
            }

            throw new Exception("Unknown type:" + joint.GetType());
        }
    }
}
