using JsonHelper;
using PhysicEngine.ExportData;
using PhysicEngine.Joints;

namespace PhysicEngine.UnitTests.TestHelper
{
    //Simuliert mehrere Timesteps und verändert dabei die Set-Werte von den Gelenken
    //Phase 1: Verändere die Set-Werte
    //Phase 2: Warte bis alles zur Ruhe kommt
    //Ziel: Am Ende soll die Scene genau so aussehen wie die Vorgabescene
    internal class JointSimulator
    {
        public class JointSetpoint
        {
            public int JointIndex; //Index aus PhysicSceneConstructorData.Joints
            public float SetValue; //MotorPosition
        }

        class Joint
        {
            private Func<float> getterFunc;
            private Action<float> setterFunc;
            public Joint(IPublicJoint joint)
            {
                if (joint is IPublicDistanceJoint)
                {
                    var j = (IPublicDistanceJoint)joint;
                    getterFunc = () => j.LengthPosition;
                    setterFunc = (setValue) => j.LengthPosition = setValue;
                }

                if (joint is IPublicRevoluteJoint)
                {
                    var j = (IPublicRevoluteJoint)joint;
                    getterFunc = () => j.MotorPosition;
                    setterFunc = (setValue) => j.MotorPosition = setValue;
                }

                if (joint is IPublicPrismaticJoint)
                {
                    var j = (IPublicPrismaticJoint)joint;
                    getterFunc = () => j.MotorPosition;
                    setterFunc = (setValue) => j.MotorPosition = setValue;
                }

                if (joint is IPublicWheelJoint)
                {
                    var j = (IPublicWheelJoint)joint;
                    getterFunc = () => j.MotorPosition;
                    setterFunc = (setValue) => j.MotorPosition = setValue;
                }
            }

            public float Value
            {
                get => this.getterFunc();
                set => this.setterFunc(value);
            }
        }

        public static float SimulateAndCompare(string startScene, string expectedEndScene, float timeStepTickRate, int phase1Steps, int phase2Steps, JointSetpoint[] setpoints)
        {
            var sceneData = ExportHelper.ReadFromFile(startScene);
            var scene = new PhysicScene(sceneData);

            var setter = setpoints.Select(x => new Joint(scene.GetAllJoints()[x.JointIndex])).ToArray();

            for (int i=0; i < phase1Steps; i++)
            {
                float t = (float)i / phase1Steps;
                for (int j=0;j<setpoints.Length; j++)
                {
                    float s1 = setter[j].Value;
                    float s2 = setpoints[j].SetValue;
                    float setValue = (1 - t) * s1 + t * s2;
                    setter[j].Value = setValue;
                }

                scene.TimeStep(timeStepTickRate);
            }

            for (int i=0;i < phase2Steps; i++)
            {
                scene.TimeStep(timeStepTickRate);
            }

            var actual = scene.GetExportData();
            var expected = Helper.FromCompactJson<PhysicSceneExportData>(File.ReadAllText(expectedEndScene));

            string testOutputFile = new FileInfo(expectedEndScene).DirectoryName + "\\Output.txt";

            File.WriteAllText(testOutputFile, Helper.ToCompactJson(actual));

            return SceneComparer.GetMaxPositionDifference(actual, expected);
        }

        
    }
}
