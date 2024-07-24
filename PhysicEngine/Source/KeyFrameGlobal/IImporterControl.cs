namespace KeyFrameGlobal
{
    public interface IImporterControl
    {
        event ImportIsFinishedHandler ImportIsFinished;
    }

    public delegate void ImportIsFinishedHandler(AnimatorInputData data);
}
