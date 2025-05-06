using LevelEditorExports.Editor.Helper;

namespace LevelEditorGlobal
{
    //Über dieses Interface können all die LevelItems und RigidBodys mit der Maus angeklickt werden
    public interface IMouseclickableWithTagData : IMouseClickable, ITagable
    {
    }

    //Das PhysicLevelItem ist der Container, welcher lauter RigidBody-Kindelemetne hat, wo Tagdaten dran gespeichert werden dürfen
    public interface ITagableContainer
    {
        IMouseclickableWithTagData[] Tagables { get; }
    }
}
