using AstroidsControl;
using CarDrifterControl;
using ElmaControl;
using LevelEditorControl;
using MoonlanderControl;
using SkiJumperControl;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace DemoApplications.UnitTests
{
    [Collection("Our Test Collection #1")] //Mit diesen Attribut wird verhindert, dass die Testklassen, die auch diesen Collectionname haben parallel laufen. Das ist nötig wegen SetResourceAssembly und dem StaFact-Attribut. https://xunit.net/docs/running-tests-in-parallel
    public class DemoGameTests
    {
        public const string GameFolder = @"..\..\..\..\..\Data\GameData\";
        public const string TestResultsFolder = @"..\..\..\..\..\Data\TestData\DemoGameTestResults\";
        public const string ExpectedFolder = @"..\..\..\..\..\Data\TestData\DemoGameTestResults\Expected\";
        public const float TimerTickRateInMs = 30; //ms

        #region Avoid "URI prefix is not recognized" and "STA-InvalidOperationException"
        //Um eine LevelEditor-Datei in eine Simulator-Datei zu konvertieren erzeuge ich das ViewModel vom LevelEditor und 
        //dieses ViewModel erzeugt sowohl ein new Uri("pack://application:,,,/LevelEditorControl;component/Controls/EditorControl/DoubleDown.png")-Objekt
        //aber auch ein WPF-UserControl.
        //Um Resourcen aus eine Dll aufzulösen muss das ResourceAssembly-Attribut gestzt werden (Siehe Schritt 1 und 2)
        //Um ein UserControl-Objekt aus ein UnitTest herraus zu erzeugen muss dem UnitTest-Runner gesagt werden, dass er
        //die Test-Methode in ein Thread oder Task starten soll, muss der Thread im STA (Single Threaded Apartment) gestaret werden: thread.SetApartmentState(ApartmentState.STA);
        //Den TestRunner von XUnit kann diese STA-Anweisung dadurch sagen, indem man das NuGet-Packet 'Xunit.StaFact' installiert und dann [StaFact] anstatt [Fact] nutzt 
        public DemoGameTests()
        {
            //Wenn ich die Tests hier laufen lasse, bekomme ich im LevelEditor folgende Exception: 
            //System.UriFormatException: "Invalid URI: Invalid port specified."
            //Das ViewModel vom Editor nutzt für die Buttons eine Bilddatei, die per Uri-Objekt auf eine Assembly-Resource verweist:
            //pack://application:,,,/LevelEditorControl;component/Controls/EditorControl/DoubleDown.png
            //Mit folgender Answeisung verhindere ich, dass die "Invalid port specified"-Exception kommt: 
            //Schritt 1: Verhindere diese Exception: System.UriFormatException: "Invalid URI: Invalid port specified."
            if (!UriParser.IsKnownScheme("pack")) //Diese Zeile verhindert, dass die Exception "System.InvalidOperationException : A URI scheme name 'pack' already has a registered custom parser." kommt, wenn man alle Tests der Klasse aufruft
                UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), "pack", -1); 

            //So geht es ab .NET 5.0
            SetResourceAssembly(typeof(LevelEditorFactory).Assembly); //Schritt 2: Verhindere diese Exception: System.NotSupportedException: "The URI prefix is not recognized."
            //So ging es bei .NET Framework: System.Windows.Application.ResourceAssembly = typeof(LevelEditorFactory).Assembly;

            //Schritt 3: Da das ViewModel vom LevelEditor ein UserControl-Objekt erzeugt, kommt die Exception: System.InvalidOperationException: "Beim aufrufenden Thread muss es sich um einen STA-Thread handeln, da dies für viele Komponenten der Benutzeroberfläche erforderlich ist."
            //Um zu verhindern, dass bei ein XUnit-Test diese Exception kommt muss das NuGet-Packet 'Xunit.StaFact' installiert werden.
            //Wenn man dann [StaFact] anstatt [Fact] über den Test schreibt, dann kommt die Exception nicht mehr. Quelle:
            //http://dontcodetired.com/blog/post/Running-xUnitnet-Tests-on-Specific-Threads-for-WPF-and-Other-UI-Tests

            //Schritt 4: Der Aufruf von SetResourceAssembly als auch die Nutzung von [StaFact] darf nicht parallel erfolgen.
            //Deswegen muss über allen Testklassen, wo SetResourceAssembly oder [StaFact] genutzt wird mit dem Attribut
            //[Collection("Our Test Collection #1")] verhindert werden, dass diese Testklassen parallel laufen.
        }

        //https://github.com/microsoft/testfx/issues/975
        public static void SetResourceAssembly(Assembly assembly)
        {
            var _resourceAssemblyField = typeof(Application).GetField("_resourceAssembly", BindingFlags.Static | BindingFlags.NonPublic);
            if (_resourceAssemblyField != null)
                _resourceAssemblyField.SetValue(null, assembly);

            var resourceAssemblyProperty = typeof(BaseUriHelper).GetProperty("ResourceAssembly", BindingFlags.Static | BindingFlags.NonPublic);
            if (resourceAssemblyProperty != null)
                resourceAssemblyProperty.SetValue(null, assembly);
        }
        #endregion

        [Fact]        
        public void Moonlander_LandsOnPlatform()
        {
            var result = TestHelper.RunSimulation(new MoonlanderControlFactory(), "Moonlander", "Moonlander.txt", "ReplayLandsOnPlatform.txt");

            result.Image.Save(TestResultsFolder + "Moonlander1.bmp");
            File.WriteAllText(TestResultsFolder + "Moonlander1_SoundLog.txt", result.SoundLog);

            Bitmap expectedImage = new Bitmap(ExpectedFolder + "Moonlander1.bmp");
            Assert.True(TestHelper.CompareTwoBitmaps(expectedImage, result.Image));

            string expectedSoundLog = File.ReadAllText(ExpectedFolder + "Moonlander1_SoundLog.txt");
            Assert.Equal(expectedSoundLog, result.SoundLog);
        }

        [Fact]
        public void Moonlander_DestroyShip()
        {
            var result = TestHelper.RunSimulation(new MoonlanderControlFactory(), "Moonlander", "Moonlander.txt", "ReplayDestroyShip.txt");

            result.Image.Save(TestResultsFolder + "Moonlander2.bmp");
            File.WriteAllText(TestResultsFolder + "Moonlander2_SoundLog.txt", result.SoundLog);

            Bitmap expectedImage = new Bitmap(ExpectedFolder + "Moonlander2.bmp");
            Assert.True(TestHelper.CompareTwoBitmaps(expectedImage, result.Image));

            string expectedSoundLog = File.ReadAllText(ExpectedFolder + "Moonlander2_SoundLog.txt");
            Assert.Equal(expectedSoundLog, result.SoundLog);
        }

        [Fact]
        public void SkiJumper_DoesBackflip()
        {
            var result = TestHelper.RunSimulation(new SkiJumperControlFactory(), "SkiJumper", null, "Backflip.txt");

            result.Image.Save(TestResultsFolder + "SkiJumper.bmp");
            File.WriteAllText(TestResultsFolder + "SkiJumper_SoundLog.txt", result.SoundLog);

            Bitmap expectedImage = new Bitmap(ExpectedFolder + "SkiJumper.bmp");
            Assert.True(TestHelper.CompareTwoBitmaps(expectedImage, result.Image));

            string expectedSoundLog = File.ReadAllText(ExpectedFolder + "SkiJumper_SoundLog.txt");
            Assert.Equal(expectedSoundLog, result.SoundLog);
        }

        [StaFact] //Für dieses Attribut benötigt man das NuGet-Packet: Xunit.StaFact. Wird benötigt, da das MainViewModel UserControls anlegt welche eine Exception im UnitTest werfen.
        public void Elma_PartyLevel()
        {
            var result = TestHelper.RunSimulation(new ElmaControlFactory(), "Elma", "01 Partylevel.txt", "01 Partylevel_UnitTest.txt");

            result.Image.Save(TestResultsFolder + "Elma.bmp");
            File.WriteAllText(TestResultsFolder + "Elma_SoundLog.txt", result.SoundLog);

            Bitmap expectedImage = new Bitmap(ExpectedFolder + "Elma.bmp");
            Assert.True(TestHelper.CompareTwoBitmaps(expectedImage, result.Image));

            string expectedSoundLog = File.ReadAllText(ExpectedFolder + "Elma_SoundLog.txt");
            Assert.Equal(expectedSoundLog, result.SoundLog);
        }

        [Fact]
        public void Astroid_DestroyStoneAndSatellit()
        {
            var result = TestHelper.RunSimulation(new AstroidsControlFactory(), "Astroids", "Astroids.txt", "LastReplay.txt");

            result.Image.Save(TestResultsFolder + "Astroids.bmp");
            File.WriteAllText(TestResultsFolder + "Astroids_SoundLog.txt", result.SoundLog);

            Bitmap expectedImage = new Bitmap(ExpectedFolder + "Astroids.bmp");
            Assert.True(TestHelper.CompareTwoBitmaps(expectedImage, result.Image));

            string expectedSoundLog = File.ReadAllText(ExpectedFolder + "Astroids_SoundLog.txt");
            Assert.Equal(expectedSoundLog, result.SoundLog);
        }

        [Fact]
        public void CarDrifter_DriftAroundTheCurve()
        {
            var result = TestHelper.RunSimulation(new CarDrifterControlFactory(), "CarDrifter", "CarDrifter.txt", "LastReplay.txt");

            result.Image.Save(TestResultsFolder + "CarDrifter.bmp");
            File.WriteAllText(TestResultsFolder + "CarDrifter_SoundLog.txt", result.SoundLog);

            Bitmap expectedImage = new Bitmap(ExpectedFolder + "CarDrifter.bmp");
            Assert.True(TestHelper.CompareTwoBitmaps(expectedImage, result.Image));

            string expectedSoundLog = File.ReadAllText(ExpectedFolder + "CarDrifter_SoundLog.txt");
            Assert.Equal(expectedSoundLog, result.SoundLog);
        }
    }
}