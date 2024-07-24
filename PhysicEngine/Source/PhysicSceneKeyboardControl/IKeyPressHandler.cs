namespace PhysicSceneKeyboardControl
{
    public interface IKeyPressHandler
    {
        int Id { get; } //index aus Returnwert von PhysicSceneKeyPressHandlerProvider.GetHandler
        string KeyPressDescription { get; }
        void HandleKeyDown();
        void HandleKeyUp();
    }
}
