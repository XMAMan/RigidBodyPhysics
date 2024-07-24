using FluentAssertions;
using Xunit;

namespace PhysicEngine.UnitTests
{
    public class ProjectedGaussSeidelTest
    {
        //So funktioniert Gauss-Seidel. 
        //Das Beispiel ist von hier: https://miro.medium.com/v2/resize:fit:828/format:webp/1*XhOO9fKIPaYGhe2wi3wFkg.png
        [Fact]
        public void SolveLinearSystemWithTwoVariables()
        {
            //Gegeben ist folgendes Gleichungssystem
            //3x + 1y = 5
            //2x + 1y = 6
            //Gesucht sind x und y
            //Bei Gauss-Seidel löße ich immer abwechselnd nach einer gesuchten Variable
            //Die Gleichungen nach x und y umstellen:
            //x=(5-y)/3
            //y=(6-2x)/2

            int iterations = 10;
            float startValueX = 0;
            float startValueY = 0;
            float x = startValueX;
            float y = startValueY;
            for (int i = 0; i < iterations; i++)
            {
                x = (5 - y) / 3;
                y = (6 - 2 * x) / 2;
            }

            //Prüfe ob es stimmt:
            float row1 = 3 * x + y;     //Erwartung für row1: 5
            float row2 = 2 * x + 2 * y; //Erwartung für row2: 6

            row1.Should().BeApproximately(5, 0.0001F);
            row2.Should().BeApproximately(6, 0.0001F);
        }
    }
}
