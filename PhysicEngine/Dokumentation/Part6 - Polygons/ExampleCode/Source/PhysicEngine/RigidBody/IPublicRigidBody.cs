using PhysicEngine.MathHelper;

namespace PhysicEngine.RigidBody
{
    public interface IPublicRigidBody : IExportableBody
    {
        Vec2D Center { get; }
        float Angle { get; }
        Vec2D Velocity { get; }
        float AngularVelocity { get; }        
    }

    public interface IPublicRigidRectangle : IPublicRigidBody
    {
        Vec2D[] Vertex { get; }
        Vec2D Size { get; }
        float GetPushPullForce();
    }

    public interface IPublicRigidCircle : IPublicRigidBody
    {        
        float Radius { get; }        
    }

    public interface IPublicRigidPolygon : IPublicRigidBody
    {
        Vec2D[] Vertex { get; }
        PolygonCollisionType PolygonType { get; }

        List<Vec2D[]> SubPolys { get; } //Zur Testausgabe der konvexen Teilpolygone (Wenn das Polygon ein RigidPolygon ist)
        bool[] IsConvex { get; } //Zur Testausgabe der IsConvex-Property beim EdgePolygon
    }

    public enum PolygonCollisionType
    {
        Rigid, //Polygon, was aus lauter konvexen Polygonen besteht, wo es selbst dann zum Schnittpunkt kommt, wenn das Objekt nicht den Rand berührt
        EdgeWithNormalsPointingInside,  //Nur wenn die Kante berührt wird kann es zur Kollision kommen. Normalen zeigen nach innen. Wird für den Levelrand benötigt.
        EdgeWithNormalsPointingOutside, //Wenn ich ein Polygon mit Löchern erzeugen will, dann ist das der Außerrand und das Loch wäre ein EdgeInside-Polygon
    }
}
