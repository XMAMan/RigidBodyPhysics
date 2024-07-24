using GraphicMinimal;

namespace LevelEditorGlobal
{
    //Wenn ein Objekt dieses Interface hat, dann weiß das TagControl, dass es für dieses Objekt ein TagName und eine TagColor speichern darf 
    //Dieses Interface wird vom PhysicPrototypItem und MouseclickableExportBody implementiert. D.h. nur Dinge, die sich auch bewegen können haben ein Tag
    public interface ITagable
    {
        int Id { get; }        
        enum TagType { Proto, Polygon, Body, Joint, Thruster, Motor, AxialFriction}
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
    
    //Bekommt der SimulatorExporter als Input
    public class EditorTagdata
    {
        public int LevelItemId { get; set; }    //ILevelItem.Id
        public int TagId { get; set; }          //ITagable.Id
        public ITagable.TagType TagType { get; set; } //ITagable.TypeName
        public string PrototypTagName { get; set; } = string.Empty;
        public byte PrototypColor { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public byte Color { get; set; } = 0;
        public Vector2D[] AnchorPoints { get; set; }
    }
}
