using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace RigidBodyPhysics.MaxForceTracking
{
    //Überwacht die PushPull-Force von den Rechtecken und die Point2Point-Kräfte von den Gelenken und löscht sie, wenn die Kräfte zu groß werden
    internal class MaxForceTracker
    {
        private List<IBreakableBody> breakableBodies;
        private List<IBreakableJoint> breakableJoints;
        private Action<IBreakableBody> maxBodyForceReachedHandler;
        private Action<IBreakableJoint> maxJointForceReachedHandler;
        public MaxForceTracker(List<IBreakableBody> breakableBodies, List<IBreakableJoint> breakableJoints, Action<IBreakableBody> maxBodyForceReachedHandler, Action<IBreakableJoint> maxJointForceReachedHandler)
        {
            this.breakableBodies = breakableBodies.Where(x => x.BreakWhenMaxPushPullForceIsReached).ToList();
            this.breakableJoints = breakableJoints.Where(x => x.BreakWhenMaxForceIsReached).ToList();
            this.maxBodyForceReachedHandler = maxBodyForceReachedHandler;
            this.maxJointForceReachedHandler = maxJointForceReachedHandler;
        }

        public void TryToAddBody(IPublicRigidBody body)
        {
            if (body is IBreakableBody && (body as IBreakableBody).BreakWhenMaxPushPullForceIsReached)
            {
                this.breakableBodies.Add((IBreakableBody)body);
            }
        }

        public void TryToAddJoint(IJoint joint)
        {
            if (joint is IBreakableJoint && (joint as IBreakableJoint).BreakWhenMaxForceIsReached)
            {
                this.breakableJoints.Add((IBreakableJoint)joint);
            }
        }

        public void RemoveBody(IBreakableBody body)
        {
            if (this.breakableBodies.Contains(body))
            {
                this.breakableBodies.Remove(body);
            }            
        }

        public void RemoveJoint(IBreakableJoint joint)
        {
            if (this.breakableJoints.Contains(joint))
            {
                this.breakableJoints.Remove(joint);
            }            
        }

        public void CheckForces()
        {
            var bodys = this.breakableBodies.ToList(); //So kann RemoveBody aufgerufen werden, während hier duch die Liste gegangen wird
            foreach (var body in bodys)
            {
                if (Math.Abs(body.GetPushPullForce()) > body.MaxPushPullForce)
                {
                    body.IsBroken = true;
                    this.maxBodyForceReachedHandler(body);
                }                    
            }

            var joints = this.breakableJoints.ToList(); //So kann RemoveJoint aufgerufen werden, während hier duch die Liste gegangen wird
            foreach (var joint in joints)
            {
                if (joint is IBreakablePushPullJoint)
                {
                    var pushPullJoint = (IBreakablePushPullJoint)joint;
                    if (joint.CurrentForce < pushPullJoint.MinForceToBreak || joint.CurrentForce > joint.MaxForceToBreak)
                    {
                        joint.IsBroken = true;
                        this.maxJointForceReachedHandler(joint);
                    }
                }else
                {
                    //CurrentForce ist ein Impulse. Durch die Dt-Division erhalte ich die Kraft
                    if (Math.Abs(joint.CurrentForce) > joint.MaxForceToBreak)
                    {
                        joint.IsBroken = true;
                        this.maxJointForceReachedHandler(joint);
                    }
                }

                
                    
            }
        }
    }
}
