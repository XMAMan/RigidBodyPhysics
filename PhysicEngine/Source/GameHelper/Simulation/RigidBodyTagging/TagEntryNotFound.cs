namespace GameHelper.Simulation.RigidBodyTagging
{
    public class TagEntryNotFound : Exception
    {
        public TagEntryNotFound()
        {
        }

        public TagEntryNotFound(string message)
            : base(message)
        {
        }

        public TagEntryNotFound(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
