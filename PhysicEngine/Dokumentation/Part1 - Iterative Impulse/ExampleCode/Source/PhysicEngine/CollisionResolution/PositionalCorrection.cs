using GraphicMinimal;
using PhysicEngine.CollisionDetection;

namespace PhysicEngine.CollisionResolution
{
    internal class PositionalCorrection
    {
        //Quelle: https://github.com/Apress/building-a-2d-physics-game-engine/blob/master/978-1-4842-2582-0_source%20code/Chapter5/Chapter5.1ACoolDemo/public_html/EngineCore/Physics.js
        //posCorrectionRate -> percentage of separation to project objects
        public static void DoCorrection(RigidBodyCollision[] collisions, float posCorrectionRate = 0.8f)
        {
            foreach (var c in collisions)
            {
                Vector2D correctionAmount = c.Normal * (c.Depth / (c.B1.InverseMass + c.B2.InverseMass) * posCorrectionRate);
                c.B1.MoveCenter(-correctionAmount * c.B1.InverseMass);
                c.B2.MoveCenter(correctionAmount * c.B2.InverseMass);
            }
        }
    }
}
