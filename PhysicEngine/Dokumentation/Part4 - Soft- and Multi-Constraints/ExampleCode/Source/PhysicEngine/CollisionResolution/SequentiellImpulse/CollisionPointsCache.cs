using PhysicEngine.CollisionDetection;

namespace PhysicEngine.CollisionResolution.SequentiellImpulse
{
    //Merkt sich all die Kollisionspunkte vom letzten TimeStep und schaut beim aktuellen Step, was neu hinzu gekommen
    //ist und was weg kann. Ziel: AccumulatedImpulse an den Kollisionspunkten über mehrere TimeSteps speichern
    internal class CollisionPointsCache
    {
        private List<CollisionPointWithImpulse> cache = new List<CollisionPointWithImpulse>();

        internal CollisionPointWithImpulse[] Update(RigidBodyCollision[] collisions)
        {
            List<CollisionPointWithImpulse> newList = new List<CollisionPointWithImpulse>();
            foreach (var c in collisions)
            {
                CollisionPointWithImpulse oldFound = null;
                foreach (var old in cache)
                {
                    if (old.Key == c.Key)
                    {
                        oldFound = old;
                        break;
                    }
                }

                var newPoint = new CollisionPointWithImpulse(c);

                if (oldFound != null)
                    newPoint.TakeDataFromOtherPoint(oldFound);

                newList.Add(newPoint);
            }

            cache = newList;
            return cache.ToArray();
        }
    }
}
