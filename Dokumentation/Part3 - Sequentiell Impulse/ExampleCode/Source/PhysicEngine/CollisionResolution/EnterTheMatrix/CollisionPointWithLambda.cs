using PhysicEngine.CollisionDetection;

namespace PhysicEngine.CollisionResolution.EnterTheMatrix
{
    //Speichert den Normal- und Friction-Lambda-Wert für ein Kollisionspunkt
    internal class CollisionPointWithLambda : RigidBodyCollision
	{
		public float NormalLambda = 0;
		public float FrictionLambda = 0;

		public CollisionPointWithLambda(RigidBodyCollision c)
			:base(c)
		{
		}

		public void TakeDataFromOtherPoint(CollisionPointWithLambda c)
		{
			this.NormalLambda = c.NormalLambda;
			this.FrictionLambda = c.FrictionLambda;
		}		
	}
}
