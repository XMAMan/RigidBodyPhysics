using RigidBodyPhysics;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace Simulator.ForceTracking
{
    enum ForceType { NoForce, PushPull, DistancePosition, DistanceMinMax, PointToPoint, MinMaxAngular, FixAngular, AngularMotor, PointToLine, MinMaxTranslation, TranslationMotor, AxialFriction }

    //Speichert für eine PhysicScene in jeden Timestep, welche Stab- und Gelenkkräfte aufgetreten sind
    //Die Daten sehen aus wie eine Tabelle mit N Spalten. Jede Spalte steht für eine Stab- oder Gelenkraft
    //Pro Zeitschritt kommt eine Tabellenzeile hinzu.
    public class ForceTracker
    {
        private IForceTracker[] tracker; //So viele Spalten hat die Tabelle
        private List<float[]> forceSamples = new List<float[]>(); //List=Enthält die Zeilen; Array=Spaltenanzahl

        public ForceTracker(PhysicScene physicScene)
        {
            this.tracker = GetAllTracker(physicScene);
        }

        #region Schritt 1: Ermittle welcher Körper/Gelenk aus der gemergeten Scene zu welchen LevelItem gehört

        interface IForceTracker
        {
            ForceType ForceType { get; }
            Func<float> GetForce { get; }
            int Index { get; }
            string Name { get; }
        }

        //Stellt den Zusammenhang zwischen der PhysisScene aus dem LevelItem mit der gemergten PhysicScene dar
        class BodyForceTracker : IForceTracker
        {
            public enum BodyType { Rectangle, Circle, Polygon }
            public BodyType Type { get; set; }
            public int Index { get; set; } //Index aus der gemergeten Scene aus PhysicScene.GetBodies()
            public ForceType ForceType { get; set; }
            public Func<float> GetForce { get; set; }
            public string Name { get => this.Type.ToString(); }
        }

        class JointForceTracker : IForceTracker
        {
            public enum JointType { Distance, Revolute, Prismatic, Weld, Wheel }
            public JointType Type { get; set; }
            public int Index { get; set; } //Index aus der gemergeten Scene aus PhysicScene.GetJoints()
            public ForceType ForceType { get; set; }
            public Func<float> GetForce { get; set; }
            public string Name { get => this.Type.ToString(); }
        }

        class AxialFrictionTracker : IForceTracker
        {
            public ForceType ForceType { get; } = ForceType.AxialFriction;
            public Func<float> GetForce { get; set; }
            public int Index { get; set; } //Index aus der gemergeten Scene aus PhysicScene.GetAxialFrictions()
            public string Name { get => this.ForceType.ToString(); }
        }

        private static IForceTracker[] GetAllTracker(PhysicScene physicScene)
        {
            List<BodyForceTracker> bodies = new List<BodyForceTracker>();
            List<JointForceTracker> joints = new List<JointForceTracker>();
            List<AxialFrictionTracker> axialFrictions = new List<AxialFrictionTracker>();

            var mergedBodys = physicScene.GetAllBodys();

            for (int i = 0; i < mergedBodys.Length; i++)
            {
                var body = mergedBodys[i];
                if (body is IPublicRigidRectangle)
                {
                    var rec = (IPublicRigidRectangle)body;
                    var bodyTracker = new BodyForceTracker()
                    {
                        Type = BodyForceTracker.BodyType.Rectangle,
                        Index = i,
                        ForceType = ForceType.PushPull,
                        GetForce = () => (rec != null ? rec.GetPushPullForce() : 0)
                    };

                    bodies.Add(bodyTracker);
                }
            }

            var mergedJoints = physicScene.GetAllJoints();
            for (int i = 0; i < mergedJoints.Length; i++)
            {
                //Zu jeden Joint gibt es N JointForces
                var jointForces = GetAllTrackerForSingleJoint(mergedJoints[i], i);
                joints.AddRange(jointForces);
            }

            var mergedAxialFrictions = physicScene.GetAllAxialFrictions();
            for (int i=0;i<mergedAxialFrictions.Length;i++)
            {
                var fri = mergedAxialFrictions[i];
                var frictionTracker = new AxialFrictionTracker()
                {
                    Index = i,
                    GetForce = () => fri.AccumulatedFrictionImpulse
                };
                axialFrictions.Add(frictionTracker);
            }

            List<IForceTracker> tracker = new List<IForceTracker>();
            tracker.AddRange(bodies);
            tracker.AddRange(joints
                //Nur diese 4 Joint-Kraftarten haben sich als nützlich in der Überwachugn erwiesen. Deswegen zeige ich nur diese auch an
                .Where(x =>
                    x.ForceType == ForceType.PushPull ||
                    x.ForceType == ForceType.PointToLine ||
                    x.ForceType == ForceType.PointToPoint ||
                    x.ForceType == ForceType.DistancePosition                    
                )
                );
            tracker.AddRange(axialFrictions);

            return tracker.ToArray();
        }

        private static JointForceTracker[] GetAllTrackerForSingleJoint(IPublicJoint joint, int mergedSceneIndex)
        {
            JointForceTracker[] tracker = null;
            if (joint is IPublicDistanceJoint)
            {
                tracker = GetAllTrackerForDistanceJoint((IPublicDistanceJoint)joint);
            }
            if (joint is IPublicRevoluteJoint)
            {
                tracker = GetAllTrackerForRevoluteJoint((IPublicRevoluteJoint)joint);
            }
            if (joint is IPublicPrismaticJoint)
            {
                tracker = GetAllTrackerForPrismaticJoint((IPublicPrismaticJoint)joint);
            }
            if (joint is IPublicWeldJoint)
            {
                tracker = GetAllTrackerForWeldJoint((IPublicWeldJoint)joint);
            }
            if (joint is IPublicWheelJoint)
            {
                tracker = GetAllTrackerForWheelJoint((IPublicWheelJoint)joint);
            }

            for (int i = 0; i < tracker.Length; i++)
            {
                tracker[i].Index = mergedSceneIndex;
            }

            return tracker;
        }

        private static JointForceTracker[] GetAllTrackerForDistanceJoint(IPublicDistanceJoint joint)
        {
            return new JointForceTracker[]
            {
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Distance,
                    ForceType = ForceType.DistancePosition,
                    GetForce = () => joint.AccumulatedImpulse
                },
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Distance,
                    ForceType = ForceType.DistanceMinMax,
                    GetForce = () => joint.AccumulatedImpulseForMinMax
                }
            };
        }
        private static JointForceTracker[] GetAllTrackerForRevoluteJoint(IPublicRevoluteJoint joint)
        {
            return new JointForceTracker[]
            {
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Revolute,
                    ForceType = ForceType.MinMaxAngular,
                    GetForce = () => joint.AccumulatedMinMaxAngularImpulse
                },
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Revolute,
                    ForceType = ForceType.AngularMotor,
                    GetForce = () => joint.AccumulatedAngularMotorImpulse
                },
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Revolute,
                    ForceType = ForceType.PointToPoint,
                    GetForce = () => joint.AccumulatedPointToPointImpulse.Length()
                }
            };
        }
        private static JointForceTracker[] GetAllTrackerForPrismaticJoint(IPublicPrismaticJoint joint)
        {
            return new JointForceTracker[]
            {
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Prismatic,
                    ForceType = ForceType.PointToLine,
                    GetForce = () => joint.AccumulatedPointToLineImpulse
                },
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Prismatic,
                    ForceType = ForceType.FixAngular,
                    GetForce = () => joint.AccumulatedAngularImpulse
                },
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Prismatic,
                    ForceType = ForceType.MinMaxTranslation,
                    GetForce = () => joint.AccumulatedMinMaxImpulse
                },
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Prismatic,
                    ForceType = ForceType.TranslationMotor,
                    GetForce = () => joint.AccumulatedTranslationMotorImpulse
                }
            };
        }
        private static JointForceTracker[] GetAllTrackerForWeldJoint(IPublicWeldJoint joint)
        {
            return new JointForceTracker[]
            {
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Weld,
                    ForceType = ForceType.PointToPoint,
                    GetForce = () => joint.AccumulatedPointToPointImpulse.Length()
                },
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Weld,
                    ForceType = ForceType.FixAngular,
                    GetForce = () => joint.AccumulatedAngularImpulse
                }
            };
        }
        private static JointForceTracker[] GetAllTrackerForWheelJoint(IPublicWheelJoint joint)
        {
            return new JointForceTracker[]
            {
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Wheel,
                    ForceType = ForceType.PointToLine,
                    GetForce = () => joint.AccumulatedPointToLineImpulse
                },
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Wheel,
                    ForceType = ForceType.MinMaxTranslation,
                    GetForce = () => joint.AccumulatedMinMaxImpulse
                },
                new JointForceTracker()
                {
                    Type = JointForceTracker.JointType.Wheel,
                    ForceType = ForceType.TranslationMotor,
                    GetForce = () => joint.AccumulatedTranslationMotorImpulse
                }
            };
        }
        #endregion

        #region Schritt 2: Speichere pro Zeitschritt für jedes Gelenk/Körper seine Kraftwerte
        public void AddDataRow()
        {
            var forceSample = this.tracker.Select(x => x.GetForce()).ToArray();
            this.forceSamples.Add(forceSample);
        }
        #endregion

        #region Schritt 3: Gemessene Daten nach außen geben

        public bool ForceDataAvailable()
        {
            return this.forceSamples.Any();
        }
        public string[] GetTrackerNames()
        {
            return this.tracker.Select(x => x.Name + "_" + x.Index /*+ "_" + x.ForceType*/).ToArray();
        }

        public float GetSingleSample(int forceIndex, int time)
        {
            return this.forceSamples[time][forceIndex];
        }

        public float[] GetAllSamplesFromASingleTracker(int forceIndex)
        {
            return this.forceSamples.Select(x => x[forceIndex]).ToArray();
        }

        #endregion        
    }
}
