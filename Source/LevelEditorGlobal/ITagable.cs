using LevelEditorExports.Simulator;

namespace LevelEditorGlobal
{
    //Wenn ein Objekt dieses Interface hat, dann weiß das TagControl, dass es für dieses Objekt ein TagName und eine TagColor speichern darf 
    //Dieses Interface wird vom PhysicPrototypItem und MouseclickableExportBody implementiert. D.h. nur Dinge, die sich auch bewegen können haben ein Tag
    public interface ITagable
    {
        int Id { get; }       
        TagType TypeName { get; } //Wird zur Anzeige der Child-Items vom PhysicLevelItem im TreeControl genutzt
    }

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
