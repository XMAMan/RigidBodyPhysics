using RigidBodyPhysics.MathHelper;

namespace RigidBodyPhysics.CollisionDetection.NearPhase
{
    internal enum CollidableType
    {
        Container = -1, // ICollidableContainer
        Circle = 0,     // ICollidableCircle
        Rectangle = 1,  // ICollidableRectangle
        Edge = 2,       // ICollideablePolygonEdge
        Polygon = 3     // IConvexPolygon
    }

    internal interface ICollidable
    {
        bool IsNotMoveable { get; } //Wenn die InverseMasse 0 ist, dann ist das Objekt nicht bewegbar
        CollidableType TypeId { get; }
        List<ICollidable> CollideExcludeList { get; } //Mit diesen Objekten soll dieses Objekt nicht kollidieren (Wird für IJoint.CollideConnected benötigt)
        int CollisionCategory { get; } //Die CollisionMatrix legt fest, welche CollisionCategory mit welcher anderen CollisionCategory kollidiert
    }

    internal interface ICollidableContainer
    {
        ICollidable[] Colliables { get; }
    }

    internal interface ICollidableCircle : ICollidable
    {
        Vec2D Center { get; }
        float Radius { get; }
    }

    internal interface IConvexPolygon : ICollidable
    {
        Vec2D Center { get; }
        Vec2D[] Vertex { get; }
        Vec2D[] FaceNormal { get; }   //[0] = Bezieht sich auf die Kante Vertex[0]-Vertex[1]; [Length-1]=Vertex[Length-1]-Vertex[0]
    }

    internal interface ICollidableRectangle : IConvexPolygon
    {
        //Kommt über die Vererbung
        //0--TopLeft;1--TopRight;2--BottomRight;3--BottomLeft
        //Vec2D[] Vertex { get; } //Kommt über die Vererbung

        //0--Top;1--Right;2--Bottom;3--Left
        //Vec2D[] FaceNormal { get; } //Kommt über die Vererbung
    }

    //Das ist eine Linie, die von P1 nach P2 geht und eine Normale hat
    internal interface ICollideablePolygonEdge : ICollidable
    {
        Vec2D Center { get; } //Massezentrum des zugehörigne Polygons
        Vec2D P1 { get; }
        Vec2D P2 { get; }
        float Min { get; } //Wenn Min = 0, dann startet die Linie bei P1. 
        float Max { get; } //Wenn Max = Length, dann endet die Linie bei P2
        Vec2D Normal { get; }
        Vec2D P1ToP2Direction { get; }
        float Length { get; }
        bool IsP1Convex { get; } //Kann der P1-Punkt eine Rechteckkante durchstoßen ohne das es weitere Kollisionspunkte zwischen den Rechteck und den Polygon gibt?
    }

    //Convexes Polygon, was dadurch entstanden ist, indem ein Konkaves Polygon in konvexe zerlegt wurde
    internal interface IConvexSubPolygon : IConvexPolygon
    {
        //Kommt über die Vererbung
        //Vec2D[] Vertex { get; } 
        //Vec2D[] FaceNormal { get; }   //[0] = Bezieht sich auf die Kante Vertex[0]-Vertex[1]; [Length-1]=Vertex[Length-1]-Vertex[0]
        bool[] IsOutsideEdge { get; } //[0] = Bezieht sich auf die Kante Vertex[0]-Vertex[1]; [Length-1]=Vertex[Length-1]-Vertex[0]
        Vec2D CenterFromParentPolygon { get; }
    }
}
