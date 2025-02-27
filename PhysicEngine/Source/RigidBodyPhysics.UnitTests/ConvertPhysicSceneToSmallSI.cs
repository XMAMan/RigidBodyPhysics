using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.UnitTests.TestHelper;
using System.Text;
using Xunit;

namespace RigidBodyPhysics.UnitTests
{
    //Schritt 1: Erzeuge mit PhysicSceneTestbed.bat eine Scene und speichere sie SmallSI\SmallSIScene.txt
    //Schritt 2: Starte im debuger die Convert-Funktion und kopiere den result-string in die Reset-Funktion von der Datei SmallSI\Source\SmallSI\PhysicWindow.cs
    public class ConvertPhysicSceneToSmallSI
    {
        public const string SmallSIFolder = @"..\..\..\..\..\..\..\SmallSI\";

        [Fact(Skip = "Do this only if the SmallSIScene.txt should be updated")]
        public void Convert()
        {
            var bodys = ExportHelper.ReadFromFile(SmallSIFolder + "SmallSIScene.txt").Bodies.Where(x => x is RigidRectangle).Cast<RigidRectangle>().ToList();

            float sizeFactor = 0.5f;

            StringBuilder str = new StringBuilder();
            foreach (var b in bodys)
            {
                string density = b.InverseMass == 0 ? "float.MaxValue" : "0.0001f";
                str.AppendLine($"physicScene.Bodies.Add(new RigidRectangle(new Vec2D({ToFloat(b.Center.X * sizeFactor)}, {ToFloat(b.Center.Y * sizeFactor)}), new Vec2D({ToFloat(b.Size.X * sizeFactor)}, {ToFloat(b.Size.Y * sizeFactor)}), {ToFloat(b.Angle)}, {density}, {ToFloat(b.Restituion)}, {ToFloat(b.Friction)}));");
            }
            string result = str.ToString();
        }

        private static string ToFloat(float x)
        {
            return x.ToString().Replace(",", ".") + "f";
        }
    }
}
