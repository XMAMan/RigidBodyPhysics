﻿namespace PhysicEngine.CollisionResolution.EnterTheMatrix.Constraints
{
    //Gibt die Zeilen von einer Velocity-Constraint für die J, Bias, Min- und Max-Lambda-Matrizen zurück
    internal interface IConstraint
    {
        Matrix GetJacobian();
        Matrix GetBias();
        Matrix GetMinLambda();
        Matrix GetMaxLambda();
        Matrix GetInitialLambda();
        int GetLambdaRowCount();
        void SaveLambdaInCollisionPoints(Matrix lambda); //Speichert die Lambdawerte in CollisionPointWithLambda
	}
}