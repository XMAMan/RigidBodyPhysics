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
}
