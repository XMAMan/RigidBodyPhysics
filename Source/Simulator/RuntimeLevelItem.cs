using PhysicSceneDrawing;
using RigidBodyPhysics.ExportData;

namespace Simulator
{
    //Speichert alle IPublic-Objekte die zu ein LevelItem gehören
    public class RuntimeLevelItem : PhysicScenePublicData
    {
        public int LevelItemId { get; }
        public ITexturedRigidBody[] Textures { get; }

        public RuntimeLevelItem(int levelItemId, PhysicScenePublicData physicData, ITexturedRigidBody[] textures)
            : base(physicData)
        {
            this.LevelItemId = levelItemId;
            this.Textures = textures;
        }
    }
}
