using PhysicEngine.CollisionDetection.NearPhase;

namespace PhysicEngine.CollisionDetection.BroadPhase
{
    //Speichert die Liste der RigidBody-Paare, welche laut der ShouldCollide-Funktion kollidieren sollen
    //Auf diese Weisen soll verhindert werden, das bei jeden Frame der Levelrand gegen sich selbst getestet wird
    internal class CollisionPairManager<T> where T : class, ICollidable, IBoundingCircle
    {
        private List<CollidablePair<T>> pairs;

        private List<T> collidables;
        public bool[,] CollisionMatrix { get; private set; }

        public CollisionPairManager(List<T> collidables, bool[,] collisionMatrix)
        {
            this.collidables = collidables;
            this.CollisionMatrix = collisionMatrix;


            UpdatePairList();
        }

        private void UpdatePairList()
        {
            this.pairs = new List<CollidablePair<T>>();

            for (int i = 0; i < collidables.Count; i++)
                for (int j = i + 1; j < collidables.Count; j++)
                {
                    var b1 = collidables[i];
                    var b2 = collidables[j];
                    if (ShouldCollide(b1, b2, CollisionMatrix))
                    {
                        this.pairs.Add(new CollidablePair<T>(b1, b2, i, j));
                    }
                }
        }


        public IEnumerable<CollidablePair<T>> GetPairs() { return pairs; }

        public void AddCollidable(T collidable)
        {
            this.collidables.Add(collidable);
            UpdatePairList();
        }

        public void RemoveCollidable(T collidable)
        {
            this.collidables.Remove(collidable);
            UpdatePairList();
        }

        public void UpdateAfterJointWasRemoved()
        {
            UpdatePairList();
        }


        //Das ist der BroadPhase-Filter. All diese ICollidable-Eigenschaften ändern sich beim Objekt über seine Laufzeit nicht.
        private static bool ShouldCollide(ICollidable b1, ICollidable b2, bool[,] collisionMatrix)
        {
            if (b1.IsNotMoveable && b2.IsNotMoveable) return false;
            if (collisionMatrix[b1.CollisionCategory, b2.CollisionCategory] == false) return false;
            if (b1.CollideExcludeList.Contains(b2)) return false;

            return true;
        }
    }

    class CollidablePair<T> where T : class, ICollidable, IBoundingCircle
    {
        public T C1 { get; }
        public T C2 { get; }
        public int Index1 { get; } //Index von C1 in der collidables-List
        public int Index2 { get; } //Index von C2 in der collidables-List

        public CollidablePair(T c1, T c2, int index1, int index2)
        {
            this.C1 = c1;
            this.C2 = c2;
            this.Index1 = index1;
            this.Index2 = index2;
        }
    }
}
