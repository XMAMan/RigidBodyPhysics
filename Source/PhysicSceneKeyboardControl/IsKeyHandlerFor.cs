using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.Thruster;

namespace PhysicSceneKeyboardControl
{
    //Erweitert die IKeyPressHandler um die IsHandlerFor-Funktion. Wird benötigt, um Gelenke/Thruster löschen zu können
    public static class IsKeyHandlerFor
    {
        public static bool IsHandlerFor(this IKeyPressHandler handler, IPublicRevoluteJoint revoluteJoint)
        {
            return handler is RevoluteJointKeyPressHandler && (handler as RevoluteJointKeyPressHandler).RevoluteJoint == revoluteJoint;
        }

        public static bool IsHandlerFor(this IKeyPressHandler handler, IPublicThruster thruster)
        {
            return handler is ThrusterKeyPressHandler && (handler as ThrusterKeyPressHandler).Thruster == thruster;
        }

        public static bool IsHandlerFor(this IKeyPressHandler handler, IPublicRotaryMotor motor)
        {
            return handler is RotaryMotorKeyPressHandler && (handler as RotaryMotorKeyPressHandler).Motor == motor;
        }
    }
}
