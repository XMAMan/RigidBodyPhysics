using LevelToSimulatorConverter;

namespace LevelEditorControl.LevelItems.GroupedItems
{
    //Enthält mehrere IPhysicMergerItem-Objekte
    internal interface IPhysicSceneContainer
    {
        IMergeablePhysicScene[] GetPhysicMergerItems();
    }
}
