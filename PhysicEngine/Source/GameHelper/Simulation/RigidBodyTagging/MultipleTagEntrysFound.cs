namespace GameHelper.Simulation.RigidBodyTagging
{
    public class MultipleTagEntrysFound : Exception
    {
        public MultipleTagEntrysFound()
        {
        }

        public MultipleTagEntrysFound(string message)
            : base(message)
        {
        }

        public MultipleTagEntrysFound(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
