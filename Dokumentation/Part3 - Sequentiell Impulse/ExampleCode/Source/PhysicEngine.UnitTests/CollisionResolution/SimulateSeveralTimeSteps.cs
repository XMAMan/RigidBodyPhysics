using GraphicMinimal;
using PhysicEngine.RigidBody;
using System.Text;

namespace PhysicEngine.UnitTests.CollisionResolution
{
    //Überwacht über mehrere Zeitschritte die Position/Geschwindigkeit und schaut, ob der Körper sich ruhig verhält
    internal class SimulateSeveralTimeSteps
    {
        public class Body
        {
            public Vector2D Velocity;
            public float AngularVelocity;
            public Vector2D Position;
            public float Angle;

            public Body(IRigidBody body)
            {
                int roundDecimalPlaces = 7;  //Runde 7 Stellen nach dem Komma damit der Float-Vergleich im UnitTest klappt
                this.Velocity = MathHelp.Round(body.Velocity, roundDecimalPlaces);
                this.AngularVelocity = MathHelp.Round(body.AngularVelocity, roundDecimalPlaces);
                this.Position = new Vector2D(body.Center);
                this.Angle = body.Angle;
            }

            public override string ToString()
            {
                return this.Velocity.ToString() + " " + this.AngularVelocity.ToString();
            }
        }

        public class TimeStep
        {
            public Body[] Bodies;

            public TimeStep(List<IRigidBody> bodies)
            {
                this.Bodies = new Body[bodies.Count];
                for (int i=0;i<bodies.Count; i++)
                    this.Bodies[i] = new Body(bodies[i]);
            }
        }

        

        public TimeStep[] TimeSteps = null;

        public class ExtraSettings
        {
            public bool DoPositionCorrection = false;
            public float AllowedPenetration = 1.0f;
            public bool DoWarmStart = true;

            public PhysicScene.SolverType? Solver = null;
        }

        public static SimulateSeveralTimeSteps DoTest(string bodysFilePath, float timeStepTickRate, bool useGlobalSolver, int pgsIterations = 100, int timeSteps = 50, ExtraSettings extraSettings = null)
        {
            SimulateSeveralTimeSteps result = new SimulateSeveralTimeSteps();
            result.TimeSteps = new TimeStep[timeSteps];

            var bodys = ExportHelper.ReadFromFile(bodysFilePath);

            var scene = new PhysicScene(bodys);
            if (useGlobalSolver) scene.UseGlobalSolver(); else scene.UseGroupSolver();
            scene.PushBodysApart(); //Erzeuge ruhige Resting-Kontaktpunkte
            scene.DoPositionalCorrection = false;
            scene.DoWarmStart = true;
            scene.HasGravity = true;
            scene.IterationCount = pgsIterations;

            if (extraSettings != null)
            {
                scene.DoPositionalCorrection = extraSettings.DoPositionCorrection;
                scene.AllowedPenetration = extraSettings.AllowedPenetration;
                scene.DoWarmStart = extraSettings.DoWarmStart;
                if (extraSettings.Solver != null) scene.Solver = (PhysicScene.SolverType)extraSettings.Solver;
            }

            result.TimeSteps[0] = new TimeStep(bodys);

            for (int i=1;i<timeSteps;i++)
            {
                scene.TimeStep(timeStepTickRate);
                result.TimeSteps[i] = new TimeStep(bodys);
            }

            return result;
        }

        //Gib für jeden Körper zurück, wie lange er am Ende der Simulation in Ruhe liegt
        public static int[] GetTimeStepCountForGettingCalmForEachBody(TimeStep[] timeSteps)
        {
            int[] calm = new int[timeSteps[0].Bodies.Length];
            for (int i = 0; i < timeSteps[0].Bodies.Length; i++)
            {
                calm[i] = timeSteps.Length - GetStepCountWherePositionIsCalm(timeSteps, i);
            }

            return calm;
        }

        //Erst fällt ein Körper zum Boden und dann kommt er zur Ruhe. Diese Funktion schaut, für wie viele TimeSteps
        //er dann in Ruhe liegt
        private static int GetStepCountWherePositionIsCalm(TimeStep[] timeSteps, int bodyIndex)
        {
            var pos = timeSteps.Select(x => x.Bodies[bodyIndex].Position).ToList();
            pos.Reverse();
            for (int i = 1; i < pos.Count; i++)
                //if (Math.Round(pos[0].X) != Math.Round(pos[i].X) || Math.Round(pos[0].Y) != Math.Round(pos[i].Y)) return i - 1;
                if (pos[0].Xi != pos[i].Xi || pos[0].Yi != pos[i].Yi) return i - 1;

            return pos.Count;
        }

        public static float[] GetMaxFloatValuesFromSingleBody(TimeStep[] timeSteps, int bodyIndex, Func<Body, float> getProperty)
        {
            float[] values = timeSteps.Select(x => getProperty(x.Bodies[bodyIndex])).ToArray();
            int[] maxIndices = SimulateSeveralTimeSteps.GetMinMaxPoints(values).Where(x => x.PointType == SimulateSeveralTimeSteps.MinMaxPoint.Type.Max).Select(x => x.Position).ToArray();
            float[] maxValues = maxIndices.Select(x => values[x]).ToArray();

            return maxValues;
        }

        public static float[] GetMinFloatValuesFromSingleBody(TimeStep[] timeSteps, int bodyIndex, Func<Body, float> getProperty)
        {
            float[] values = timeSteps.Select(x => getProperty(x.Bodies[bodyIndex])).ToArray();
            int[] maxIndices = SimulateSeveralTimeSteps.GetMinMaxPoints(values).Where(x => x.PointType == SimulateSeveralTimeSteps.MinMaxPoint.Type.Min).Select(x => x.Position).ToArray();
            float[] maxValues = maxIndices.Select(x => values[x]).ToArray();

            return maxValues;
        }

        //Gibt eine Tabelle mit den Spalten "Position.X Position.Y Velocity.X Velocity.Y AngualarVelocity" und pro Zeile ein TimeStep zurück
        public string GetTableFromBody(int bodyIndex)
        {
            var table = this.TimeSteps
                .Select(x => x.Bodies[bodyIndex])
                .Select(x => x.Position.X + "\t" + x.Position.Y + "\t" + x.Velocity.X + "\t" + x.Velocity.Y + "\t" + x.AngularVelocity)
                .ToArray();

            StringBuilder str = new StringBuilder();
            str.AppendLine("TimeStep\tX\tY\tV.X\tV.Y\tW");
            for (int i = 0; i < table.Length; i++)
            {
                str.AppendLine(i + "\t" + table[i].ToString());
            }

            return str.ToString();
        }

        public int BodyCount { get => this.TimeSteps[0].Bodies.Length; }

        //Source: https://stackoverflow.com/questions/5166716/linq-to-calculate-a-moving-average-of-a-sortedlistdatetime-double
        public static IEnumerable<double> MovingAverage(IEnumerable<double> source, int period)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (period < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(period));
            }

            return Core();

            IEnumerable<double> Core()
            {
                var sum = 0.0;
                var buffer = new double[period];
                var n = 0;
                foreach (var x in source)
                {
                    n++;
                    sum += x;
                    var index = n % period;
                    if (n >= period)
                    {
                        sum -= buffer[index];
                        yield return sum / period;
                    }

                    buffer[index] = x;
                }
            }
        }

        public float[] Differentiate(float[] signal)
        {
            float[] f = new float[signal.Length - 1];
            for (int i=0; i<f.Length; i++)
                f[i] = signal[i + 1] - signal[i];

            return f;
        }

        public class MinMaxPoint
        {
            public int Position;
            public enum Type { Min, Max };
            public Type PointType = Type.Min;

            public MinMaxPoint(int position, Type pointType)
            {
                this.Position = position;
                this.PointType = pointType;
            }
        }
        public static MinMaxPoint[] GetMinMaxPoints(float[] signal)
        {
            List<MinMaxPoint> points = new List< MinMaxPoint >();

            int sign = Sign(signal[1] - signal[0]); //1 = Steigend; -1 = Fallend
            for (int i=1; i<signal.Length - 1; i++)
            {
                int s = Sign(signal[i + 1] - signal[i]);
                if (s != sign)
                {
                    if (sign == 1 && s == -1)
                        points.Add(new MinMaxPoint(i, MinMaxPoint.Type.Max));
                    else
                        points.Add(new MinMaxPoint(i, MinMaxPoint.Type.Min));

                }
                sign = s;
            }

            return points.ToArray();
        }

        private static int Sign(float f)
        {
            return f >= 0 ? 1 : -1;
        }

        public class PointWithMinMax
        {
            public float Value;
            public float Min = float.NaN;
            public float Max = float.NaN;
        }

        //Gibt die Min-Max-Hüllkurve für ein Signal zurück
        public static PointWithMinMax[] GetEnvelope(float[] signal)
        {
            PointWithMinMax[] points = signal.Select(x => new PointWithMinMax() { Value = x }).ToArray();

            var minMax = GetMinMaxPoints(signal);            
            foreach (var p in minMax)
            {
                if (p.PointType == MinMaxPoint.Type.Min)
                    points[p.Position].Min = signal[p.Position];

                if (p.PointType == MinMaxPoint.Type.Max)
                    points[p.Position].Max = signal[p.Position];
            }

            return points;
        }

        public static string GetSignalWithEnvelope(float[] signal)
        {
            var points = GetEnvelope(signal);

            StringBuilder str = new StringBuilder();
            str.AppendLine("Value\tMin\tMax");
            for (int i = 0; i < points.Length; i++)
                str.AppendLine(points[i].Value + "\t" + (float.IsNaN(points[i].Min) ? "" : points[i].Min) + "\t" + (float.IsNaN(points[i].Max) ? "" : points[i].Max));

            return str.ToString();
        }

        public static float GetMinMaxRangeFromLastNEntrys(float[] signal, int n)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            for (int i=signal.Length -n; i< signal.Length; i++)
            {
                if (signal[i] < min) min = signal[i];
                if (signal[i] > max) max = signal[i];
            }
            return max - min;
        }
    }
}
