using LevelEditorGlobal;
using RigidBodyPhysics.RuntimeObjects.AxialFriction;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace GameHelper.Simulation.RigidBodyTagging
{
    public interface ITagDataProvider
    {
        IEnumerable<IPublicRigidBody> GetBodiesByTagName(string tagName);
        IEnumerable<IPublicRigidBody> GetBodiesByTagName(int levelItemId, string tagName);
        IPublicRigidBody GetBodyByTagName(string tagName);
        IPublicRigidBody GetBodyByTagName(int levelItemId, string tagName);
        PhysicSceneTagdataEntry GetTagDataFromBody(IPublicRigidBody body);
        IEnumerable<IPublicJoint> GetJointsByTagName(string tagName);
        IEnumerable<IPublicJoint> GetJointsByTagName(int levelItemId, string tagName);
        IPublicJoint GetJointByTagName(string tagName);
        IPublicJoint GetJointByTagName(int levelItemId, string tagName);
        PhysicSceneTagdataEntry GetTagDataFromJoint(IPublicJoint joint);
        IEnumerable<IPublicThruster> GetThrustersByTagName(string tagName);
        IPublicThruster GetThrusterByTagName(string tagName);
        IPublicThruster GetThrusterByTagName(int levelItemId, string tagName);
        PhysicSceneTagdataEntry GetTagDataFromThruster(IPublicThruster thruster);
        IEnumerable<IPublicRotaryMotor> GetMotorsByTagName(string tagName);
        IPublicRotaryMotor GetMotorByTagName(string tagName);
        IPublicRotaryMotor GetMotorByTagName(int levelItemId, string tagName);
        PhysicSceneTagdataEntry GetTagDataFromMotor(IPublicRotaryMotor motor);
        IEnumerable<IPublicAxialFriction> GetAxialFrictionsByTagName(string tagName);
        IPublicAxialFriction GetAxialFrictionByTagName(string tagName);
        IPublicAxialFriction GetAxialFrictionByTagName(int levelItemId, string tagName);
        PhysicSceneTagdataEntry GetTagDataFromAxialFriction(IPublicAxialFriction axialFriction);
    }
}
