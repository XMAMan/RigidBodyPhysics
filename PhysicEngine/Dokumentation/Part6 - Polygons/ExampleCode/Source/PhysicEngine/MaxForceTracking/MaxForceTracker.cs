namespace PhysicEngine.MaxForceTracking
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

        public void CheckForces()
        {
            foreach (var body in breakableBodies)
            {
                if (Math.Abs(body.GetPushPullForce()) > body.MaxPushPullForce)
                    this.maxBodyForceReachedHandler(body);
            }

            foreach (var joint in breakableJoints)
            {
                //CurrentForce ist ein Impulse. Durch die Dt-Division erhalte ich die Kraft
                if (Math.Abs(joint.CurrentForce) > joint.MaxForceToBreak)
                    this.maxJointForceReachedHandler(joint);
            }
        }
    }
}
