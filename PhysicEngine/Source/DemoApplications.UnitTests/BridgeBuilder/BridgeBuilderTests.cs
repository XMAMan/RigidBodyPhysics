using BridgeBuilderControl.Controls.Simulator.Model.Converter;
using JsonHelper;
using LevelEditorControl;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using static DemoApplications.UnitTests.BridgeBuilder.SingleBridgeTestHelper;
using SingelResult = DemoApplications.UnitTests.BridgeBuilder.SingleBridgeTestHelper.SingleBridgeResult;

namespace DemoApplications.UnitTests.BridgeBuilder
{
    [Collection("Our Test Collection #1")] //Mit diesen Attribut wird verhindert, dass die Testklassen, die auch diesen Collectionname haben parallel laufen. Das ist nötig wegen SetResourceAssembly und dem StaFact-Attribut. https://xunit.net/docs/running-tests-in-parallel
    public class BridgeBuilderTests
    {
        public const string TestResultsFolder = @"..\..\..\..\..\Data\TestData\DemoGameTestResults\BridgeBuilder\";
        public const string BridgesSolutions = @"..\..\..\..\..\Data\TestData\DemoGameTestResults\BridgeBuilder\Bridges_Solution\";
        public const string UserBridges = @"..\..\..\..\..\Data\TestData\DemoGameTestResults\BridgeBuilder\UserBridges\";


        #region Avoid "URI prefix is not recognized" and "STA-InvalidOperationException"
        //Um eine LevelEditor-Datei in eine Simulator-Datei zu konvertieren erzeuge ich das ViewModel vom LevelEditor und 
        //dieses ViewModel erzeugt sowohl ein new Uri("pack://application:,,,/LevelEditorControl;component/Controls/EditorControl/DoubleDown.png")-Objekt
        //aber auch ein WPF-UserControl.
        //Um Resourcen aus eine Dll aufzulösen muss das ResourceAssembly-Attribut gestzt werden (Siehe Schritt 1 und 2)
        //Um ein UserControl-Objekt aus ein UnitTest herraus zu erzeugen muss dem UnitTest-Runner gesagt werden, dass er
        //die Test-Methode in ein Thread oder Task starten soll, muss der Thread im STA (Single Threaded Apartment) gestaret werden: thread.SetApartmentState(ApartmentState.STA);
        //Den TestRunner von XUnit kann diese STA-Anweisung dadurch sagen, indem man das NuGet-Packet 'Xunit.StaFact' installiert und dann [StaFact] anstatt [Fact] nutzt 
        public BridgeBuilderTests()
        {
            //Wenn ich die Tests hier laufen lasse, bekomme ich im LevelEditor folgende Exception: 
            //System.UriFormatException: "Invalid URI: Invalid port specified."
            //Das ViewModel vom Editor nutzt für die Buttons eine Bilddatei, die per Uri-Objekt auf eine Assembly-Resource verweist:
            //pack://application:,,,/LevelEditorControl;component/Controls/EditorControl/DoubleDown.png
            //Mit folgender Answeisung verhindere ich, dass die "Invalid port specified"-Exception kommt: 
            if (!UriParser.IsKnownScheme("pack")) //Diese Zeile verhindert, dass die Exception "System.InvalidOperationException : A URI scheme name 'pack' already has a registered custom parser." kommt, wenn man alle Tests der Klasse aufruft
                UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), "pack", -1); //Schritt 1: Verhindere diese Exception: System.UriFormatException: "Invalid URI: Invalid port specified."

            //So geht es ab .NET 5.0
            SetResourceAssembly(typeof(LevelEditorFactory).Assembly); //Schritt 2: Verhindere diese Exception: System.NotSupportedException: "The URI prefix is not recognized."
            //So ging es bei .NET Framework: System.Windows.Application.ResourceAssembly = typeof(LevelEditorFactory).Assembly;

            //Schritt 3: Da das ViewModel vom LevelEditor ein UserControl-Objekt erzeugt, kommt die Exception: System.InvalidOperationException: "Beim aufrufenden Thread muss es sich um einen STA-Thread handeln, da dies für viele Komponenten der Benutzeroberfläche erforderlich ist."
            //Um zu verhindern, dass bei ein XUnit-Test diese Exception kommt muss das NuGet-Packet 'Xunit.StaFact' installiert werden.
            //Wenn man dann [StaFact] anstatt [Fact] über den Test schreibt, dann kommt die Exception nicht mehr. Quelle:
            //http://dontcodetired.com/blog/post/Running-xUnitnet-Tests-on-Specific-Threads-for-WPF-and-Other-UI-Tests
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

        //Wenn die Brücke zu stabil ist, ist das Spiel zu leicht und wenn sie so unstabil ist, dass das 
        //Level nicht schaffbar ist, dann ist das Spiel zu schwer. Diese Tests hier ermitteln in welchen
        //Min-Max-Bereich die MaxPushForce/MaxPullForce liegen muss, damit es bei Level 1 und 2 sich korrekt verhält

        enum Limit 
        { 
            MinPush,    //Legt fest, wie groß die Druckkraft mindestens sein muss, damit die Brücke nicht zu labil ist
            MaxPush,    //Legt fest, wie groß die Druckkraft maximal sein darf, damit die Brücke nicht zu stabil ist
            MinPull,    //Legt fest, wie groß die Zugkraft mindestens sein muss, damit die Brücke nicht zu labil ist
            MaxPull     //Legt fest, wie groß die Zugkraft maximal sein darf damit die Brücke nicht zu stabil ist
        }

        //Schritt 1: Ermittle die maximalen Push-Pull-Kräfte ohne das ein Zug drüber fährt
        //Hiermit wird BridgeConverterSettings.MaxPushForce/MaxPullForce ermittelt
        //Außerdem wird der Test rot, wenn RectangleDensity zu groß ist. Somit kann man hier den Maximalwert für RectangleDensity bestimmen
        [Fact(Skip = "Wird nicht mehr gebraucht da PushPullLimits.txt für jedes Level die Kräfte angibt")]
        public void GetAllowedRangeForPushPull_NoTrain() //NoTrain -> Brücke wo kein Zug drüber fährt
        {
            KeyValuePair<string, Limit>[] testData = new KeyValuePair<string, Limit>[]
           {
                new KeyValuePair<string, Limit>("Level1_NoTrain_Pull_Fail.txt", Limit.MaxPull),
                new KeyValuePair<string, Limit>("Level1_NoTrain_Pull_Ok.txt", Limit.MinPull),
                new KeyValuePair<string, Limit>("Level1_NoTrain_Push_Fail.txt", Limit.MaxPush),
                new KeyValuePair<string, Limit>("Level1_NoTrain_Push_OK.txt", Limit.MinPush),
                new KeyValuePair<string, Limit>("Level2_NoTrain_Pull_Fail.txt", Limit.MaxPull),
                new KeyValuePair<string, Limit>("Level2_NoTrain_Pull_OK.txt", Limit.MinPull),
           };

            float minPush = float.MinValue;
            float maxPush = float.MaxValue;
            float minPull = float.MaxValue;
            float maxPull = float.MinValue;

            foreach (var test in testData)
            {
                var resultData = SingleBridgeTestHelper.SimulateSingleBridge(new SingleBridgeInputData()
                {
                    BridgeFile = UserBridges + test.Key,
                    FramesToWaitForTrain = 0,
                    MaxFramesToWaitForFinish = -100,
                    Settings = new BridgeConverterSettings() { BridgeIsBreakable = false, PositionalCorrectionRate = 0.1f }
                });

                switch (test.Value)
                {
                    case Limit.MinPush:
                        minPush = Math.Max(minPush, resultData.MaxPushForce); 
                        break;

                    case Limit.MaxPush:
                        maxPush = Math.Min(maxPush, resultData.MaxPushForce);
                        break;

                    case Limit.MinPull:
                        minPull = Math.Min(minPull, resultData.MaxPullForce);
                        break;

                    case Limit.MaxPull:
                        maxPull = Math.Max(maxPull, resultData.MaxPullForce);
                        break;
                }
            }

            Assert.True(minPush <= maxPush);
            Assert.True(maxPull <= minPull);

            //Möglichkeit 1:
            //BridgeConverterSettings.MaxPushForce = minPush
            //BridgeConverterSettings.MaxPullForce = maxPull (Etwas abrunden)

            //Möglichkeit 2: Diesen Weg nutze ich
            float pushForce = (maxPush + minPush) / 2; //BridgeConverterSettings.MaxPushForce = pushForce
            float pullForce = (maxPull + minPull) / 2; //BridgeConverterSettings.MaxPullForce = pullForce
        }

        //Hier wird geprüft, dass die im GetAllowedRangeForPushPull_NoTrain-Test ermittelten MaxPullForce/MaxPushForce-Werte korrekt sind
        [Fact(Skip = "Wird nicht mehr gebraucht da PushPullLimits.txt für jedes Level die Kräfte angibt")]
        public void TestLevel1And2NoTrain()
        {
            KeyValuePair<string, bool>[] testData = new KeyValuePair<string, bool>[]
           {
                new KeyValuePair<string, bool>("Level1_NoTrain_Pull_Fail.txt", true),
                new KeyValuePair<string, bool>("Level1_NoTrain_Pull_Ok.txt", false),
                new KeyValuePair<string, bool>("Level1_NoTrain_Push_Fail.txt", true),
                new KeyValuePair<string, bool>("Level1_NoTrain_Push_OK.txt", false),
                new KeyValuePair<string, bool>("Level2_NoTrain_Pull_Fail.txt", true),
                new KeyValuePair<string, bool>("Level2_NoTrain_Pull_OK.txt", false),
           };

            StringBuilder lines = new StringBuilder();
            lines.AppendLine($"{"File".PadRight(30)}\t{"MaxPull".PadRight(15)}\t{"MaxPush".PadRight(15)}\tResult");
            List<bool> matches = new List<bool>();
            foreach (var test in testData)
            {
                var resultData = SingleBridgeTestHelper.SimulateSingleBridge(new SingleBridgeInputData()
                {
                    BridgeFile = UserBridges + test.Key,
                    FramesToWaitForTrain = 0,
                    MaxFramesToWaitForFinish = -100,
                    Settings = new BridgeConverterSettings() { PositionalCorrectionRate = 0.1f}
                });

                bool match = resultData.SomeBarsAreBroken == test.Value ? true : false;
                string matchString = match ? "OK" : "FAIL";
                lines.AppendLine(test.Key.PadRight(30) + "\t" + resultData.MaxPullForce.ToString().PadRight(15) + "\t" + resultData.MaxPushForce.ToString().PadRight(15) + "\t" + matchString);
                matches.Add(match);
            }

            File.WriteAllText(TestResultsFolder + "Level1And2NoTrain.txt", lines.ToString());
            Assert.True(matches.All(x => x == true));
        }

        //Nachdem die Brücke ohne den Zug getestet wurde soll hier nun das Gewicht des
        //Zuges (BridgeConverterSettings.TrainDensity) ermittelt werden
        //damit die Brücke nicht zu stabil aber auch nicht zu labil ist. Der Zug wird so leicht gemacht, dass alle OK-Tests
        //zuerst grün werden und dann wird er durch probieren schrittweise schwerer gemacht, bis alle Fail-Tests grün sind
        [Fact(Skip = "Wird nicht mehr gebraucht da PushPullLimits.txt für jedes Level die Kräfte angibt")]
        public void TestLevel1And2WithTrain()
        {
            KeyValuePair<string, SingelResult>[] testData = new KeyValuePair<string, SingelResult>[]
            {
                new KeyValuePair<string, SingelResult>("Level1_WithTrain_Pull_Fail.txt", SingelResult.LevelFailed),
                new KeyValuePair<string, SingelResult>("Level1_WithTrain_Pull_OK.txt", SingelResult.LevelPassed),
                new KeyValuePair<string, SingelResult>("Level2_WithTrain_Pull_Fail.txt", SingelResult.LevelFailed),
                //new KeyValuePair<string, SingelResult>("Level2_WithTrain_Pull_Fail2.txt", SingelResult.LevelFailed),
                new KeyValuePair<string, SingelResult>("Level2_WithTrain_Pull_OK.txt", SingelResult.LevelPassed),
                //new KeyValuePair<string, SingelResult>("Level2_WithTrain_Push_Fail.txt", SingelResult.LevelFailed),
                new KeyValuePair<string, SingelResult>("Level2_WithTrain_Push_OK.txt", SingelResult.LevelPassed),
            };

            StringBuilder lines = new StringBuilder();
            lines.AppendLine($"{"File".PadRight(30)}\t{"MaxPull".PadRight(15)}\t{"MaxPush".PadRight(15)}\tResult");
            List<bool> matches = new List<bool>();
            foreach (var test in testData)
            {
                var resultData = SingleBridgeTestHelper.SimulateSingleBridge(new SingleBridgeInputData()
                {
                    BridgeFile = UserBridges + test.Key,
                    FramesToWaitForTrain = 10,
                    MaxFramesToWaitForFinish = 1000,
                    Settings = new BridgeConverterSettings()
                });
                
                bool match = false;
                if (test.Value == SingelResult.LevelPassed)
                {
                    match = resultData.BridgeResult == test.Value ? true : false;
                }else
                {
                    match = resultData.BridgeResult ==  SingelResult.LevelFailed || resultData.BridgeResult == SingelResult.Timeout ? true : false;
                }
                string matchString = match ? "OK" : "FAIL";
                lines.AppendLine(test.Key.PadRight(30) + "\t" + resultData.MaxPullForce.ToString().PadRight(15) + "\t" + resultData.MaxPushForce.ToString().PadRight(15) + "\t" + matchString);
                matches.Add(match);
            }

            File.WriteAllText(TestResultsFolder + "Level1And2WithTrain.txt", lines.ToString());
            Assert.True(matches.All(x => x == true));
        }

        //Hier werden für alle Brücken die Push-Pull-Kräfte sowohl als Grafik als auch textuell ermittelt
        //Kopiere die hier erstellte PushPullLimits.txt in den Data-Folder vom Spiel damit es dort dann verwendet wird
        [Fact (Skip= "Run this test only if you want to create a new PushPullLimits.txt-File")]
        public void CreatePushPullLimitsFile()
        {
            string[] bridgeFiles = new string[]
            {
                "Level1.txt", "Level2.txt", "Level3.txt", "Level4.txt", "Level5.txt", "Level6.txt", "Level7.txt", "Level8.txt", "Level9.txt", "Level10.txt", "Level11.txt", "Level12.txt", "Level13.txt", "Level14.txt", "Level15.txt"
            };
            int[] timeToWaitForTrain = new int[]
            {
                2, 2, 50, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            };

            var settings = new BridgeConverterSettings() { BridgeIsBreakable = false };
            var result = MultipleBridgeTestHelper.SimulateMultipleBridges(bridgeFiles.Select(x => BridgesSolutions + x).ToArray(), timeToWaitForTrain, settings);

            //File.WriteAllText(TestResultsFolder + "AllLevelForces.txt", Helper.ToJson(result));
            //var superPlot = ForcePlotter.PlotAllLevels(Helper.CreateFromJson<MultipleBridgeTestHelper.TestResult>(File.ReadAllText(TestResultsFolder + "AllLevelForces.txt")));

            var superPlot = ForcePlotter.PlotAllLevels(result);
            superPlot.Save(TestResultsFolder + "AllForces.bmp");
            File.WriteAllText(TestResultsFolder + "AllForces.txt", result.ToString());

            File.WriteAllText(TestResultsFolder + "PushPullLimits.txt", Helper.ToJson(result.AllLimits));
        }


        //Wenn bridgeIsBreakable= false, dann kann man hiermit für jeden TimeStep die maximamlen Push-Pull-Kräfte ermitteln.
        [Fact] //Für dieses Attribut benötigt man das NuGet-Packet: Xunit.StaFact
        public void TestAllLevels()
        {
            bool bridgeIsBreakable = true;

            string[] bridgeFiles = new string[]
            {
                "Level1.txt", "Level2.txt", "Level3.txt", "Level4.txt", "Level5.txt", "Level6.txt", "Level7.txt", "Level8.txt", "Level9.txt", "Level10.txt", "Level11.txt", "Level12.txt", "Level13.txt", "Level14.txt", "Level15.txt"
            };

            //Die Brücken halten teilweise nur ganz knapp. Es hat ein Einfluß, wie viele TimerTicks man
            //am Anfang wartet, bevor der Zug startet. Durch Probieren habe ich hier die Werte ermittelt,
            //wo der Zug dann ohne Einsturz über die Brücke kommt.
            int[] timeToWaitForTrain = new int[]
            {
                2, 2, 50, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            };

            var settings = new BridgeConverterSettings() { BridgeIsBreakable = bridgeIsBreakable };

            var result = MultipleBridgeTestHelper.SimulateMultipleBridges(bridgeFiles.Select(x => BridgesSolutions + x).ToArray(), timeToWaitForTrain, settings);

            File.WriteAllText(TestResultsFolder + "TestRun.txt", result.ToString());
            //AddMultiTestRun(result, settings); //Wenn ich beim Probieren alte Werte sehen will

            Assert.Equal(SingelResult.LevelPassed, result.OverallResult.BridgeResult);
        }

        
        private static void AddMultiTestRun(MultipleBridgeTestHelper.TestResult result, BridgeConverterSettings settings)
        {
            string file = TestResultsFolder + "TestRuns.txt";
            if (File.Exists(file) == false)
            {
                string header = "Result\tMaxPullForce\tMaxPushForce\tTrainDensity\tTrainSpeed\tPullForceLimit\tPushForceLimit\tBridgeIsBreakable";
                File.WriteAllText(file, header + "\r\n");
            }

            var r = result.OverallResult;
            var s = settings;
            string line = r.BridgeResult + "\t" + r.MaxPullForce + "\t" + r.MaxPushForce + "\t" + s.TrainDensity + "\t" + s.TrainSpeed + "\t" + s.MaxPullForce + "\t" + s.MaxPushForce+ "\t" + s.BridgeIsBreakable;
            File.AppendAllLines(file, new string[] { line });
        }
    }
}
