namespace BridgeBuilderControl.Testing
{
    public delegate void FirstBarIsBroken(float force, float maxPull, float maxPush);

    //Hiermit wird das Spiel testbar gemacht
    public interface IBridgeSimulator
    {
        event FirstBarIsBroken OnFirstBarIsBroken;
        void RunTrain();
        bool TrainHasPassedTheBridge();
        bool TrainIsInWater();
        void DoTimeStep(float dt);
        float GetMaxPullForce();
        float GetMaxPushForce();
        bool SomeBarsAreBroken();
        float[] GetPullForcesForEachTimeStep();
        float[] GetPushForcesForEachTimeStep();
        float GetMaxAllowedPullForce();
        float GetMaxAllowedPushForce();
    }
}
