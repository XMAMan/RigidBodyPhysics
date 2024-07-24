using PhysicEngine.CollisionDetection.BroadPhase;
using PhysicEngine.CollisionDetection.NearPhase;
using PhysicEngine.RigidBody;

namespace PhysicEngine.CollisionDetection
{
    //Ermittelt zuerst, welche RigidBody-Paare miteinander für ein NearPhase-Test in Betracht kommen und gibt dann die
    //Kollisionspunkte von allen Paaren zurück.
    internal class CollisionManager
    {
        private CollisionPairManager<IRigidBody> pairManager;

        public bool[,] CollisionMatrix { get => this.pairManager.CollisionMatrix; }

        public CollisionManager(List<IRigidBody> bodies, bool[,] collisionMatrix)
        {
            this.pairManager = new CollisionPairManager<IRigidBody>(bodies, collisionMatrix);
        }

        public RigidBodyCollision[] GetAllCollisions()
        {
            List<RigidBodyCollision> collisions = new List<RigidBodyCollision>();

            foreach (var pair in this.pairManager.GetPairs()) //BroudPhase-Filter: Gib nur die Paare zurück, die laut CollisionMatrix kollidieren können
            {
                var b1 = pair.C1;
                var b2 = pair.C2;

                if (BoundingCircleTest.Collide(b1, b2)) //Broudphase-Test
                {
                    var contacts = GetCollisions(b1, b2); //Nearphase-Test
                    if (contacts.Any())
                        collisions.AddRange(contacts.Select(x => new RigidBodyCollision(x, b1, b2, pair.Index1, pair.Index2)));
                }
            }

            return collisions.ToArray();
        }

        public void AddRigidBody(IRigidBody body)
        {
            this.pairManager.AddCollidable(body);
        }

        public void RemoveRigidBody(IRigidBody body)
        {
            this.pairManager.RemoveCollidable(body);
        }

        public void UpdateAfterJointWasRemoved()
        {
            this.pairManager.UpdateAfterJointWasRemoved();
        }

        private static CollisionInfo[] GetCollisions(ICollidable c1, ICollidable c2)
        {
            if ((c1 is ICollidableContainer) == false && (c2 is ICollidableContainer) == false) //Single with Single
            {
                return NearPhaseTests.Collide(c1, c2);
            }
            else if ((c1 is ICollidableContainer) && (c2 is ICollidableContainer) == false) //Container with Single
            {
                return (c1 as ICollidableContainer).Colliables.SelectMany(x => NearPhaseTests.Collide(x, c2)).ToArray();
            }
            else if ((c1 is ICollidableContainer) == false && (c2 is ICollidableContainer)) //Single with Container
            {
                return (c2 as ICollidableContainer).Colliables.SelectMany(x => NearPhaseTests.Collide(c1, x)).ToArray();
            }
            else //Container with Container
            {
                List<CollisionInfo> collisions = new List<CollisionInfo>();
                var container1 = c1 as ICollidableContainer;
                var container2 = c2 as ICollidableContainer;
                foreach (var single1 in container1.Colliables)
                    foreach (var single2 in container2.Colliables)
                    {
                        collisions.AddRange(NearPhaseTests.Collide(single1, single2));
                    }

                return collisions.ToArray();
            }
        }
    }
}
