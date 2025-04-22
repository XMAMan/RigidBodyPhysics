using RigidBodyPhysics.ExportData;

namespace Simulator
{
    //Speichert alle IPublic-Objekte die zu ein LevelItem gehören
    public class RuntimeLevelItem : PhysicScenePublicData
    {
        public int LevelItemId { get; }

        public RuntimeLevelItem(int levelItemId, PhysicScenePublicData physicData)
            : base(physicData)
        {
            this.LevelItemId = levelItemId;
        }
    }
}
