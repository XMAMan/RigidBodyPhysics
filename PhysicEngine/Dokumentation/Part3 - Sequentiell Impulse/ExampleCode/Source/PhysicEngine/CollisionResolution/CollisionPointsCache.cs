using PhysicEngine.CollisionDetection;

namespace PhysicEngine.CollisionResolution
{
	//Merkt sich all die Kollisionspunkte vom letzten TimeStep und schaut beim aktuellen Step, was neu hinzu gekommen
	//ist und was weg kann. Ziel: Informationen(Lambdas) an den Kollisionspunkten über mehrere TimeSteps speichern
	internal class CollisionPointsCache<T> where T : RigidBodyCollision
	{
		private List<T> cache = new List<T>();
		private Func<RigidBodyCollision, T> cToTConverter; //Wandelt ein RigidBodyCollision in ein T um
		private Action<T, T> takeDataFromOldPoint; //Nimmt vom alten T den Lambdawert und kopiert ihn zum neuen T

		public CollisionPointsCache(Func<RigidBodyCollision, T> cToTConverter, Action<T, T> takeDataFromOldPoint)
		{
			this.cToTConverter = cToTConverter;
			this.takeDataFromOldPoint = takeDataFromOldPoint;
		}

		public T[] Update(RigidBodyCollision[] collisions)
		{
			List<T> newList = new List<T>();
			foreach (var c in collisions)
			{
				T oldFound = null;
				foreach (var old in this.cache)
				{
					if (old.Key == c.Key)
					{
						oldFound = old;
						break;
					}
				}

				var newT = cToTConverter(c);

				if (oldFound == null)
					newList.Add(newT);
				else
				{
					this.takeDataFromOldPoint(oldFound, newT);
					newList.Add(newT);
				}
			}

			this.cache = newList;
			return this.cache.ToArray();
		}
	}
}
