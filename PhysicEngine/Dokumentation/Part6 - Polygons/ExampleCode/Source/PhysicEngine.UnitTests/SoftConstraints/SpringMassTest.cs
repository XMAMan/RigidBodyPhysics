using System.Text;
using Xunit;

namespace PhysicEngine.UnitTests.SoftConstraints
{
    //Ich versuche die Formeln für eine ungedämpfte Feder-Schwingung hier zu verstehen und wie ich das auf Softconstraints übertragen kann
    public class SpringMassTest
    {
        [Fact]
        public void UndampedSpring_CompareSolvers()
        {
            List<float[]> helper = new List<float[]>();
            helper.Add(UndampedSpringWithExplicitFormular());
            helper.Add(UndampedSpringWithImplicitEuler());
            helper.Add(UndampedSpringWithSemiImplicitEuler());
            helper.Add(UndampedSpringWithSemiImplicitEulerAndSoftConstraint(0));
            helper.Add(UndampedSpringWithSemiImplicitEulerAndSoftConstraint(2));

            StringBuilder str = new StringBuilder();
            str.AppendLine("ExplicitFormular\tImplicitEuler\tSemiImplicitEuler\tSemiImplicitEulerAndSoftConstraint_v2_x2\tSemiImplicitEulerAndSoftConstraint_x2_v2");
            for (int i = 0; i < helper[0].Length; i++)
            {
                for (int j = 0; j < helper.Count; j++)
                {
                    str.Append(helper[j][i] + "\t");
                }
                str.AppendLine();
            }
            string result = str.ToString();
        }

        //Mit dieser Funktion prüfe ich, ob die explizite Formel von https://de.wikipedia.org/wiki/Harmonischer_Oszillator
        //die gleichen Zahlen erzeugt wie die Euler-Verfahren weiter unten. Ergebnis: Ja das tut sie
        //m = Masse in Kg; xStart = Auslenkung der Feder zum Zeitpunkt t=0
        public float[] UndampedSpringWithExplicitFormular(float frequency = 1, float xStart = 100, float m = 1, float h = 50)
        {
            float omega = 2 * (float)Math.PI * frequency / 1000; // Durch 1000, da ich die Zeit in ms und nicht s angebe

            List<float> xValues = new List<float>();
            for (float t = 0; t <= 3000; t += h)
            {
                float x = xStart * (float)Math.Cos(omega * t);
                xValues.Add(x);
            }

            return xValues.ToArray();
        }

        //So simuliert man per Implicit-Euler eine Feder ohne Dämpfung. Ergebnis: Die Feder schwingt unendlich so wie gewünscht.
        //Soft Constraints - Erin Catto 2011 Seite 16: Implicit Euler(Feder ohne Dämpfung)
        // 1: x2 = x1 + h * v2
        // 2: v2 = v1 - h * k/m * x2  -> Das ist die Federgleichung ohne Dämpfung

        //So erfolgt die Herleitung für v2 was ohne x2-Bezug auskommt
        // v2 = v1 - h * k / m * (x1 + h * v2)              -> Setze in Gleichung 2 für x2 Gleichung 1 ein
        // v2 = v1 - h * k / m * x1 - h * k / m * h * v2    -> Klammer ausmultiplizieren
        // v2 + h* k/m* h*v2 = v1 - h * k/m * x1	        -> v2-Terme auf die linke Seite
        // v2* (1 + h* k/m* h) = v1 - h* k/m* x1	        -> v2 ausklammern
        // v2 = (v1 - h* k/m*x1)   /    (1 + h²*k/m)        -> 
        //m = Masse in Kg; xStart = Auslenkung der Feder zum Zeitpunkt t=0
        public float[] UndampedSpringWithImplicitEuler(float frequency = 1, float xStart = 100, float m = 1, float h = 50)
        {
            float omega = 2 * (float)Math.PI * frequency / 1000;
            float k = m * omega * omega;
            float x = xStart;
            float v = 0;
            float v2 = v;   //Wenn man das als Startwert nimmt, dann erhält man Werte von -100 .. +100
            //float v2 = (v - h * k / m * x) / (1 + h * h * k / m); //Mit diesen Startwert erhält man Werte von -90 .. +90

            List<float> xValues = new List<float>();
            for (float t = 0; t <= 3000; t += h)
            {
                //Soft Constraints - Erin Catto 2011 -> Seite 16 -> Hier steht die Formel für Implicit Euler
                float x2 = x + h * v2;
                //v2 = (v - h * k / m * x) / (1 + h * h * k / m); //v2-Formel ohne x2-Bezug -> Funktioniert
                v2 = v - h * k / m * x2;                          //Original-V2-Formel -> Funktioniert auch

                x = x2;
                v = v2;
                xValues.Add(x);
            }

            return xValues.ToArray();
        }

        //Diese Funktion zeigt, wie man per Semi-Implicit-Euler eine ungedämpfte Feder simulieren kann. 
        //Sie macht das gleiche wie wenn ich ResolverHelper1 nutze (PhysicScene.ResolverHelper=Helper.Helper1)
        //Die Funktion zeigt, dass wenn man per Semi-Implicit-Euler ohne SoftConstraints simuliert, dann funktioniert die
        //Simulation und die Feder schwingt unendlich.
        //Herleitung für v2 ohne x2-Bezug:
        //1: v2 = v1 + h * (F / m)
        //2: x2 = x1 + h * v2
        // F = -k * x -> Für x wird x1 eingesetzt
        //v2 = v - h * k / m * x
        //m = Masse in Kg; xStart = Auslenkung der Feder zum Zeitpunkt t=0
        public float[] UndampedSpringWithSemiImplicitEuler(float frequency = 1, float xStart = 100, float m = 1, float h = 50)
        {
            //v2 = v1 - h * k / m * x1
            //x2 = x1 + h * v2
            float omega = 2 * (float)Math.PI * frequency / 1000;
            float k = m * omega * omega;
            float x = xStart;
            float v = 0;

            List<float> xValues = new List<float>();
            for (float t = 0; t <= 3000; t += h)
            {
                //https://en.wikipedia.org/wiki/Semi-implicit_Euler_method -> Hier steht die Formel für x2/v2
                //Soft Constraints - Erin Catto 2011 -> Seite 17 -> Hier steht die Formel auch
                float v2 = v - h * k / m * x; //Semi-Implicit-Euler
                float x2 = x + h * v2;

                x = x2;
                v = v2;

                xValues.Add(x);
            }

            return xValues.ToArray();
        }

        //Diese Funktion zeigt das ich zuerst x2 ausrechnen muss und dann erst v2, wenn ich will, dass die ungedämpfte Feder
        //endllos schwingt, wenn ich sie mit Semi-Implicit-Euler unter Einsatz von SoftConstraints simuliere
        //Da aber der Jumping-Ball-Test mit Restituion=1 nur dann funktioniert, wenn man erst v2 ausrechent und dann erst x2,
        //bedeutet dass, das Semi-Implicit-Euler unter Nutzung von SoftConstraints keine unendliche Federschwingung hinbekommt.
        //Wenn man aber Semi-Implicit-Euler ohne SoftConstraints nutzt, dann funktionert es. D.h. Semi-Implicit-Euler verträgt 
        //sich nicht so richtig mit SoftConstraints, da man so keine unendliche Federschwingung hinbekommt.
        //Box2D nutzt auch den Semi-Implicit-Euler-Ansatz unter Nutzung von SoftConstraints und hat vermutlich das gleiche Problem.
        //Dieser Beitrag hier ist ein Hinweis das sie das Problem haben/hatten:
        //  https://gamedev.stackexchange.com/questions/98679/in-box2d-how-do-i-create-an-infinitely-oscillating-spring
        //Herleitung für v2 ohne Lambda/F-Bezug:
        //1: v2 = v1 + h * (F / m)
        //2: x2 = x1 + h * v2
        //3: v2 + Beta / h * x1 + Gamma * Lambda / h = 0->Das ist unsere Softconstraint - Gleichung mit dem Constraintimpuls Lambda

        // a: Lambda = -(v2 + Beta / h * x1) * h / Gamma               -> Gleichung 3 wurde nach Lambda umgestellt
        // b: F * h = Lambda                                           -> Constraintkraft * h = Constraintimpuls
        // c: -(v2 + Beta / h * x1) / Gamma = F                        -> Setze Gleichung a und b gleich und kürze h auf beiden Seiten weg

        // Setze Gleichung c für F in Gleichung 1 ein

        // v2 = v1 - h* (v2 + Beta/h* x1) / (Gamma* m)				-> Klammer ausmultiplizieren
        // v2 = v1 - h* v2 / (Gamma* m) - h* Beta/h* x1/(Gamma* m)  -> v2-Terme auf die linke Seite
        // v2 +  h* v2 / (Gamma* m) = v1 - Beta* x1/(Gamma* m)	    -> v2 ausklammern
        // v2* (1 + h / (Gamma* m)) = v1 - Beta* x1/(Gamma* m)
        // v2 = (v1 - Beta * x1 / (Gamma * m)) / (1 + h / (Gamma * m)) -> Soft Constraints - Erin Catto 2011 Seite 33 rechte Formel
        //m = Masse in Kg; xStart = Auslenkung der Feder zum Zeitpunkt t=0
        public float[] UndampedSpringWithSemiImplicitEulerAndSoftConstraint(byte variation = 0, float frequency = 1, float xStart = 100, float m = 1, float h = 50)
        {
            //v2 = (v1 - Beta * x1 / (Gamma * m)) / (1 + h / (Gamma * m))
            //x2 = x1 + h * v2
            float omega = 2 * (float)Math.PI * frequency / 1000;
            float k = m * omega * omega;
            float dampingRatio = 0;
            float c = 2 * m * dampingRatio * omega;
            float gamma = 1 / (c + h * k);
            float beta = (h * k) / (c + h * k);

            float x = xStart;
            float v = 0;
            float v2 = v;

            List<float> xValues = new List<float>();
            for (float t = 0; t <= 3000; t += h)
            {
                float x2 = float.NaN;
                switch (variation)
                {
                    case 0:
                        {
                            //Variante 1: Erst v2 ausrechnen und dann x2 (So wird es in der Physikengine verwendet)
                            //-> Die Feder schwingt nicht endlos obwohl sie ungedämpft ist
                            v2 = (v - beta * x / (gamma * m)) / (1 + h / (gamma * m)); //Semi-Implicit-Euler mit Softconstraint
                            x2 = x + h * v2;
                        }
                        break;

                    case 1:
                        {
                            //Variante 2: Erst x2 ausrechnen und dann v2
                            //-> Die Feder schwingt oder Energieverlust so wie ich es erwarte
                            x2 = x + h * v2;
                            v2 = (v - beta * x / (gamma * m)) / (1 + h / (gamma * m)); //Semi-Implicit-Euler mit Softconstraint
                        }
                        break;

                    case 2:
                        {
                            //Variante 3: v2 merken und dass dann in der x2-Formel nutzen (Enspricht Variante 2)
                            float v2Old = v2;
                            v2 = (v - beta * x / (gamma * m)) / (1 + h / (gamma * m)); //Semi-Implicit-Euler mit Softconstraint
                            x2 = x + h * v2Old;
                        }
                        break;
                }

                x = x2;
                v = v2;

                xValues.Add(x);
            }

            return xValues.ToArray();
        }
    }
}
