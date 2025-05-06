using LevelToSimulatorConverter._2_MergeToSingleScene;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    //Enthält mehrere IPhysicMergerItem-Objekte
    internal interface IPhysicSceneContainer
    {
        IMergeablePhysicScene[] GetPhysicMergerItems();
    }
}
