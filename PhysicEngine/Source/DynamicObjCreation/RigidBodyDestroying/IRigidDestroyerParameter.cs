using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using static DynamicObjCreation.RigidBodyDestroying.IRigidDestroyerParameter;

namespace DynamicObjCreation.RigidBodyDestroying
{
    public interface IRigidDestroyerParameter
    {
        public enum DestroyMethod { SingleBox, Boxes, Voronoi }
        public DestroyMethod Method { get; }

        //body = Dieses Objekt soll zerlegt werden; polyObj = Das ist ein Teilobjekt, was durch Zerlegung von body entstanden ist
        public delegate PolygonExportData Transform(IPublicRigidBody body, PolygonExportData polyObj);

        //Mit dieser Funktion kann das neu erzeugte Bruchstück-Objekt noch modifiziert werden.
        //Beispiel: Anfangsgewschwindigkeit festlegen
        Transform TransformFunc { get; set; } 

    }

    public static class RigidDestroyerDefaultParameterFactory
    {
        public static IRigidDestroyerParameter CreateParameterWithDefaulValues(IRigidDestroyerParameter.DestroyMethod method)
        {
            switch (method)
            {
                case DestroyMethod.SingleBox:
                    return new DestroyWithSingleBoxParameter();

                case DestroyMethod.Boxes:
                    return new DestroyWithBoxesParameter();

                case DestroyMethod.Voronoi:
                    return new DestroyWithVoronoiParameter();
            }

            throw new NotImplementedException();
        }
    }

    public abstract class DestroyRigidBodyParameterBase : IRigidDestroyerParameter
    {
        //Default: Nutze für die Bruchstücke die gleiche Geschwindigkeit wie für das Elternelement woraus die Stücke erzeugt wurden
        public abstract DestroyMethod Method { get; }
        public Transform TransformFunc { get; set; } = (body, polyObj) => polyObj;
    }

    public class DestroyWithSingleBoxParameter : DestroyRigidBodyParameterBase, IRigidDestroyerParameter
    {
        public override IRigidDestroyerParameter.DestroyMethod Method => IRigidDestroyerParameter.DestroyMethod.SingleBox;

    }

    public class DestroyWithBoxesParameter : DestroyRigidBodyParameterBase, IRigidDestroyerParameter
    {
        public override IRigidDestroyerParameter.DestroyMethod Method => IRigidDestroyerParameter.DestroyMethod.Boxes;
        public int BoxCount { get; set; } = 10;              
    }

    public class DestroyWithVoronoiParameter : DestroyRigidBodyParameterBase, IRigidDestroyerParameter
    {
        public override IRigidDestroyerParameter.DestroyMethod Method => IRigidDestroyerParameter.DestroyMethod.Voronoi;
        public int CellCount { get; set; } = 10;
        public Random Rand { get; set; } = new Random(0);
    }
}
