using BridgeBuilderControl.Controls.BridgeEditor.Model;
using BridgeBuilderControl.Controls.Helper;
using BridgeBuilderControl.Controls.LevelEditor;
using DynamicData;
using DynamicObjCreation;
using GameHelper.Simulation;
using GraphicMinimal;
using LevelEditorControl;
using LevelEditorGlobal;
using LevelToSimulatorConverter;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.Joints;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TextureEditorGlobal;
using static RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint;

namespace BridgeBuilderControl.Controls.Simulator.Model.Converter
{
    //Konvertiert ein SimulatorInput-Objekt in ein GameSimulator-Objekt
    internal static class EditorToSimulatorConverter
    {
        public static float BarWidth = 0.4f;
        private static bool BridgeIsVisible = false;
        public static string BridgeDistanceTagName = "BridgeDistance";
        public static string BridgeDistanceStreetTagName = "Street";
        public static string BridgeDistanceNoStreetTagName = "NoStreet";

        private static float Restitution = 0.2f;
        private static float Friction = 1000;
        private static float CircleDensity = 25;        

        private static BridgeConverterSettings Settings;
        private static LevelExport LevelExport;


        public static GameSimulator Convert(SimulatorInput input, BridgeConverterSettings settings, string dataFolder, Size panelSize, float timerIntercallInMilliseconds, out PhysikLevelItemExportData trainExportData)
        {
            Settings = settings;
            LevelExport = input.LevelExport;

            dataFolder = new DirectoryInfo(dataFolder).FullName;

            string levelFile = dataFolder + "\\Train.txt";
            var simulator = new GameSimulator(EditorFileConverter.Convert(levelFile), panelSize, input.Camera, timerIntercallInMilliseconds);

            string pushPullLimitFile = dataFolder + "\\PushPullLimits.txt";
            var limits = JsonHelper.Helper.CreateFromJson<PushPullLimit[]>(File.ReadAllText(pushPullLimitFile));
            string levelFileShort = new FileInfo(input.LevelFile).Name;
            var limit = limits.FirstOrDefault(x => x.LevelFile == levelFileShort && x.BreakAfterNSteps == settings.BreakAfterNSteps);
            if (limit != null)
            {
                float extraFactor = 1.2f;
                settings.MaxPullForce = limit.MaxPullForce * extraFactor;
                settings.MaxPushForce = limit.MaxPushForce * extraFactor;
            }

            int trainId = simulator.GetTagDataFromBody(simulator.GetBodiesByTagName("Train").First()).LevelItemId;
            var trainExport = simulator.GetExportDataFromLevelItem(trainId);
            var trainBox = PhysicSceneExportDataHelper.GetBoundingBoxFromScene(trainExport.PhysicSceneData);
            float groundHeight = GetGroundPolygonPoints(input.LevelExport).Where(x => x.X < trainBox.Max.X).Min(x => x.Y);
            float trainBottom = trainBox.Max.Y;
            foreach (var body in trainExport.PhysicSceneData.Bodies)
            {
                body.Restituion = Restitution;
                body.Friction = Friction;
                body.MassData.Density = Settings.TrainDensity;
                if (body is CircleExportData)
                {
                    //(body as CircleExportData).Radius *= 2; //Größe der Räder vom Zug
                }
            }
            foreach (var joint in trainExport.PhysicSceneData.Joints)
            {
                if (joint is RevoluteJointExportData)
                {
                    var revolute = (RevoluteJointExportData)joint;
                    revolute.MotorSpeed = Settings.TrainSpeed + LevelExport.TrainExtraSpeed;
                }
            }

            simulator.RemoveLevelItem(trainId); //Entferne den Zug mit falscher Y-Koordinate

            //Setze den Zug genau auf dem Boden auf
            PhysicSceneExportDataHelper.TranslateScene(trainExport.PhysicSceneData, Matrix4x4.Translate(0, groundHeight - trainBottom, 0));
            trainExportData = trainExport;

            int bridgeAndGroundId = simulator.AddLevelItem(GetBridgeAndGround(input, dataFolder, out ExportObject[] exportObjects));

            simulator.JointWasDeletedHandler += (sender, joint) =>
            {
                //Wenn ein Distanzjoint kaputt geht, dann sollen die zwei Rechtecke (bilden die Straße),
                //die da mit dranhängen auch gelöscht werden
                if (joint is IPublicDistanceJoint)
                {
                    var tagData = simulator.GetTagDataFromJoint(joint);
                    string tagName = simulator.GetTagDataFromJoint(joint).Names[1];
                    var rectangles = simulator.GetBodiesByTagName(tagName).ToArray();
                    foreach (var rec in rectangles)
                    {
                        simulator.RemoveRigidBody(rec);
                    }
                }
                

            };

            //Da die Scene in ein sehr kleinen Bereich ist muss auch die AllowedPenetration (Default ist 1) kleiner
            //sein, da sonst der Zug im Boden einsinkt und dort nicht mehr raus gedrückt wird.
            simulator.AllowedPenetration = 0.2f;
            simulator.PositionalCorrectionRate = Settings.PositionalCorrectionRate;

            return simulator;
        }


        #region Ground-Polygon

        private static Vec2D[] GetGroundPolygonPoints(LevelExport lev)
        {
            var groundPolyPoints = DrawingHelper
                .GetGroundPolygon(lev.GroundPolygon, lev.GroundHeight, lev.XCount, lev.YCount)
                .Select(x => x.ToPhx() - new Vec2D(0, BarWidth / 2)) //Verschiebe den Boden um eine halbe Stangenbreite nach oben damit es keine Stufe zwischen dem Boden und der der Brücke gibt
                .ToArray();

            return groundPolyPoints;
        }

        private static ExportObject GetGroundPolygon(SimulatorInput input, string dataFolder)
        {
            var polyData = GetGroundBody(input.LevelExport);
            var polyTex = GetGroundTexture(polyData, dataFolder);
            var anchorPoints = GetGroundAnchorPoints(input.LevelExport, polyData.Center);

            return new ExportObject()
            {
                ExportRigidBody = polyData,
                TextureExportData = polyTex,
                AnchorPoints = anchorPoints
            };
        }

        private static PolygonExportData GetGroundBody(LevelExport lev)
        {
            var groundPolyPoints = GetGroundPolygonPoints(lev);

            var groundCenter = PolygonHelper.GetCenterOfMassFromPolygon(groundPolyPoints);

            var groundPoly = new PolygonExportData()
            {
                PolygonType = RigidBodyPhysics.RuntimeObjects.RigidBody.PolygonCollisionType.Rigid,
                Center = groundCenter,
                Points = groundPolyPoints.Select(x => x - groundCenter).ToArray(),
                MassData = new MassData() { Type = MassData.MassType.Infinity },
                Restituion = Restitution,
                Friction = Friction,
                CollisionCategory = 0
            };

            return groundPoly;
        }

        private static TextureExportData GetGroundTexture(PolygonExportData polyData, string dataFolder)
        {
            var polyBox = PolygonHelper.GetBoundingBoxFromPolygon(polyData.Points.Select(x => polyData.Center + x).ToArray());
            string groundTexture = dataFolder + "\\Ground.png";
            return new TextureExportData() { ColorFactor = Color.White, Width = polyBox.Width, Height = polyBox.Height, TextureFile = groundTexture, ZValue = 5 };
        }

        private static AnchorPoint[] GetGroundAnchorPoints(LevelExport lev, Vec2D polygonCenter)
        {
            List<AnchorPoint> anchorPoints = new List<AnchorPoint>();
            foreach (var point in lev.AnchorPoints)
            {
                anchorPoints.Add(new AnchorPoint(point, polygonCenter, 0));
            }
            return anchorPoints.ToArray();
        }
        #endregion

        #region Bridge-Rectangles

        //Gibt die Brückenstangen zurück, welche auf der Ground-Line liegen (Dort fährt der Zug dann drüber)
        private static ExportObject[] GetBridgeRectangles(SimulatorInput input)
        {
            return input
                .BridgeExport
                .Bars
                .Where(x => IsGroundHeightBar(x, input.LevelExport.GroundHeight))
                .SelectMany(bar => GetBridgeRectangles(bar, input.LevelExport.GroundHeight)).ToArray();
        }

        private static ExportObject[] GetBridgeRectangles(Bar bar, uint groundHeight)
        {
            var body1 = GetBridgeRectangleBody(bar, 0, 2/3f);
            var body2 = GetBridgeRectangleBody(bar, 1/3f, 1);
            var tex1 = GetBridgeRectangleTexture(body1);
            var tex2 = GetBridgeRectangleTexture(body2);

            bool isStreet = IsGroundHeightBar(bar, groundHeight);
            //0=Ground;1=Zug;2=Bridge wo der Zug drüber fährt;3=Bridge wo der Zug nicht drüber fährt
            int collisionCategory = isStreet ? 2 : 3;
            body1.CollisionCategory = body2.CollisionCategory = collisionCategory;

            return new ExportObject[]
            {
                new ExportObject()
                {
                    ExportRigidBody = body1,
                    TextureExportData = tex1,
                    AnchorPoints = new AnchorPoint[]
                    {
                        new AnchorPoint(bar.P1, body1.Center, body1.AngleInDegree),      
                        null
                    },
                    Bar = bar,
                },
                new ExportObject()
                {
                    ExportRigidBody = body2,
                    TextureExportData = tex2,
                    AnchorPoints = new AnchorPoint[]
                    {
                        null,
                        new AnchorPoint(bar.P2, body2.Center, body2.AngleInDegree)
                    },
                    Bar = bar,
                }
            };
        }

        private static RectangleExportData GetBridgeRectangleBody(Bar bar, float start, float end)
        {
            float barWidth = BarWidth; //So breit ist die Stange

            var p1 = new Vec2D(bar.P1.X, bar.P1.Y);
            var p2 = new Vec2D(bar.P2.X, bar.P2.Y);
           
            var p1Start = (1 - start) * p1 + start * p2;
            var p2End = (1 - end) * p1 + end * p2;
            p1 = p1Start;
            p2 = p2End;

            float length = (p2 - p1).Length();
            var direction = (p2 - p1).Normalize();

            var center = (p1 + p2) / 2;
            float angleInDegree = Vec2D.Angle360(new Vec2D(1, 0), direction);

            var body = new RectangleExportData()
            {
                Size = new Vec2D(length + barWidth * 2, barWidth),
                Center = center,
                AngleInDegree = angleInDegree,
                Velocity = new Vec2D(0, 0),
                AngularVelocity = 0,
                MassData = new MassData(MassData.MassType.Density, 1, Settings.RectangleDensity),
                Friction = Friction,
                Restituion = Restitution,
                CollisionCategory = 2 //0=Ground;1=Zug;2=Bridge wo der Zug drüber fährt;3=Bridge wo der Zug nicht drüber fährt
            };

            return body;
        }

        private static bool IsGroundHeightBar(Bar bar, uint groundHeight)
        {
            return bar.P1.Y == groundHeight && bar.P2.Y == groundHeight;
        }

        private static TextureExportData GetBridgeRectangleTexture(RectangleExportData barData)
        {
            bool isStreet = barData.CollisionCategory == 2;

            return new TextureExportData()
            {
                ColorFactor = Color.White,
                Width = barData.Size.X,
                Height = barData.Size.Y,
                ZValue = isStreet ? 0 : 3,
                IsInvisible = !BridgeIsVisible
            };

        }

        #endregion

        #region Bridge-Circles

        //Endstücke von den Brückenstangen, die nicht auf dem Boden oder auf der Straße enden
        private static ExportObject[] GetBridgeCircles(SimulatorInput input, string dataFolder, AnchorPoint[] groundAnchorPoints)
        {
            var barEndPoints = input.BridgeExport.Bars.SelectMany(bar => GetCirclesFromBar(bar, dataFolder)).ToArray();

            //Entferne doppelte Punkte
            barEndPoints = barEndPoints.GroupBy(x => x.AnchorPoints[0].Id).Select(x => x.First()).ToArray();

            //Entferne Punkte die im Boden sind
            return barEndPoints.Where(x => groundAnchorPoints.Any(y => x.AnchorPoints[0].Id == y.Id) == false).ToArray();
        }

        private static ExportObject[] GetCirclesFromBar(Bar bar, string dataFolder)
        {
            var bodys = GetCircleBodys(bar);
            var textures = bodys.Select(x => GetBridgeBarCircleTexture(x, dataFolder)).ToArray();
            var anchorPoints = GetBridgeBarCircleAnchorPoints(bar, bodys[0], bodys[1]);

            return new ExportObject[]
            {
                new ExportObject(){ExportRigidBody = bodys[0], TextureExportData = textures[0], AnchorPoints = new AnchorPoint[]{ anchorPoints[0] } },
                new ExportObject(){ExportRigidBody = bodys[1], TextureExportData = textures[1], AnchorPoints = new AnchorPoint[]{ anchorPoints[1] } },
            };
        }

        private static CircleExportData[] GetCircleBodys(Bar bar)
        {
            var p1 = new Vec2D(bar.P1.X, bar.P1.Y);
            var p2 = new Vec2D(bar.P2.X, bar.P2.Y);

            var body1 = GetBridgeBarEndpoint(p1);
            var body2 = GetBridgeBarEndpoint(p2);

            return new CircleExportData[] {body1, body2};
        }

        private static CircleExportData GetBridgeBarEndpoint(Vec2D position)
        {
            return new CircleExportData()
            {
                Radius = BarWidth,
                Center = position,
                AngleInDegree = 0,
                Velocity = new Vec2D(0, 0),
                AngularVelocity = 0,
                MassData = new MassData(MassData.MassType.Density, 1, CircleDensity),
                Friction = 10,
                Restituion = 0.2f,
                CollisionCategory = 3, //0=Ground;1=Zug;2=Bridge wo der Zug drüber fährt;3=Bridge wo der Zug nicht drüber fährt
            };
        }
        private static TextureExportData GetBridgeBarCircleTexture(CircleExportData barEndpoint, string dataFolder)
        {
            return new TextureExportData()
            {
                ColorFactor = Color.White,
                Width = barEndpoint.Radius,
                Height = barEndpoint.Radius,
                ZValue = 3,
                IsInvisible = !BridgeIsVisible
            };

        }

        private static AnchorPoint[] GetBridgeBarCircleAnchorPoints(Bar bar, CircleExportData p1, CircleExportData p2)
        {
            return new AnchorPoint[]
            {
                new AnchorPoint(bar.P1, p1.Center, p1.AngleInDegree),
                new AnchorPoint(bar.P2, p2.Center, p2.AngleInDegree),
            };
        }

        #endregion

        #region Bridge-Distance-Joints

        //Verküpfe alle BridgeCircles mit anderen BridgeCircles/BridgeRectangles
        private static DistanceJointExportData[] GetDistanceJoints(ExportObject[] objs, Bar[] bars)
        {
            List<DistanceJointExportData> joints = new List<DistanceJointExportData>();

            foreach (Bar bar in bars)
            {
                string id1 = bar.P1.X + "_" + bar.P1.Y;
                var obj1 = objs.FirstOrDefault(x => x.AnchorPoints.Any(y => y.Id == id1));
                if (obj1 == null) continue;
                var a1 = obj1.AnchorPoints.First(x => x.Id == id1);

                string id2 = bar.P2.X + "_" + bar.P2.Y;
                var obj2 = objs.FirstOrDefault(x => x.AnchorPoints.Any(y => y.Id == id2));
                if (obj2 == null) continue;
                var a2 = obj2.AnchorPoints.First(x => x.Id == id2);

                var exportJoint = new DistanceJointExportData()
                {
                    BodyIndex1 = objs.IndexOf(obj1),
                    BodyIndex2 = objs.IndexOf(obj2),
                    R1 = a1.R1,
                    R2 = a2.R1,
                    CollideConnected = false,
                    LimitIsEnabled = true,
                    MinLength = 0,
                    MaxLength = (a2.Position - a1.Position).Length(),
                    SoftData = new SoftExportData()
                    {
                        ParameterType = SpringParameter.StiffnessAndDamping,
                        Stiffness = 0.01f,
                        Damping = 1.1f
                    },
                    MinForceToBreak = Settings.MaxPullForce,
                    MaxForceToBreak = Settings.MaxPushForce,
                    BreakWhenMaxForceIsReached = false,
                };
                joints.Add(exportJoint);
            }

            return joints.ToArray();
        }

        #endregion

        #region Tagging
        private static PhysicSceneTagdataEntry[] GetDistanceJointsTags(List<IExportJoint> joints, DistanceJointExportData[] distanceJoints, int levelItemId, uint groundHeight, Bar[] bars)
        {
            List<PhysicSceneTagdataEntry> tags = new List<PhysicSceneTagdataEntry>();

            foreach (var joint in distanceJoints)
            {
                var bar = bars[distanceJoints.IndexOf(joint)];
                var tagData = new PhysicSceneTagdataEntry(
                    ITagable.TagType.Joint,
                    levelItemId,
                    joints.IndexOf(joint),
                    new string[] { BridgeDistanceTagName, BarToString(bar), IsGroundHeightBar(bar, groundHeight) ? BridgeDistanceStreetTagName : BridgeDistanceNoStreetTagName },
                    0,
                    new Vector2D[0]
                    );
                tags.Add(tagData);
            }

            return tags.ToArray();
        }

        private static PhysicSceneTagdataEntry[] GetRectangleTags(List<IExportRigidBody> bodies, ExportObject[] rectangles, int levelItemId)
        {
            List<PhysicSceneTagdataEntry> tags = new List<PhysicSceneTagdataEntry>();

            foreach (var rec in rectangles)
            {
                var tagData = new PhysicSceneTagdataEntry(
                    ITagable.TagType.Body,
                    levelItemId,
                    bodies.IndexOf(rec.ExportRigidBody),
                    new string[] { BarToString(rec.Bar) },
                    0,
                    new Vector2D[0]
                    );
                tags.Add(tagData);
            }

            return tags.ToArray();
        }

        private static string BarToString(Bar bar)
        {
            string id1 = bar.P1.X + "_" + bar.P1.Y;
            string id2 = bar.P2.X + "_" + bar.P2.Y;
            return id1 + "#" + id2;
        }
        #endregion

        #region Bridge-RevoluteJoints

        //Verknüpfe die BridgeRectangles mit dem GroundPolygon und BridgeCircles
        private static RevoluteJointExportData[] GetRevoluteJoints(ExportObject[] objs)
        {
            List<RevoluteJointExportData> joints = new List<RevoluteJointExportData>();

            for (int i = 0; i < objs.Length - 1; i++)
                for (int j = i + 1; j < objs.Length; j++)
                {
                    joints.AddRange(TryToConnect(objs[i], objs[j], i, j));
                }

            return joints.ToArray();
        }

        private static RevoluteJointExportData[] TryToConnect(ExportObject obj1, ExportObject obj2, int i, int j)
        {
            List<RevoluteJointExportData> joints = new List<RevoluteJointExportData>();
            foreach (var a1 in obj1.AnchorPoints)
            {
                foreach (var a2 in obj2.AnchorPoints.Where(x => a1?.Id == x?.Id))
                {
                    if (a1 == null || a2 == null) continue;

                    var exportJoint = new RevoluteJointExportData()
                    {
                        BodyIndex1 = i,
                        BodyIndex2 = j,
                        R1 = a1.R1,
                        R2 = a2.R1,
                        CollideConnected = false,
                        LimitIsEnabled = false,
                        Motor = RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint.AngularMotor.Disabled,
                        SoftData = new RigidBodyPhysics.ExportData.SoftExportData() { ParameterType = RigidBodyPhysics.RuntimeObjects.Joints.IPublicJoint.SpringParameter.NoSoftness }
                        //SoftData = new SoftExportData() { ParameterType = IPublicJoint.SpringParameter.StiffnessAndDamping, Stiffness = 20, Damping = 0.5f}
                    };
                    joints.Add(exportJoint);
                }
            }

            return joints.ToArray();
        }
        #endregion

        #region Bridge-PrismaticJoints

        private static PrismaticJointExportData[] GetPrismaticJoints(ExportObject[] objs, Bar[] bars, uint groundHeight)
        {
            var rectangles = objs
                .Where(x => x.ExportRigidBody is RectangleExportData)
                .ToArray();

            var groundBars = bars
                .Where(x => IsGroundHeightBar(x, groundHeight))
                .ToArray();

            var allRigidBodys = objs.Select(x => x.ExportRigidBody).ToArray();

            List<PrismaticJointExportData> joins = new List<PrismaticJointExportData>();
            foreach (var bar in groundBars)
            {
                var recs = GetTwoRectangles(rectangles, bar);
                var joint = GetPrismaticJoint(allRigidBodys, (RectangleExportData)recs[0].ExportRigidBody, (RectangleExportData)recs[1].ExportRigidBody);
                joins.Add(joint);
            }
            return joins.ToArray();
        }

        private static ExportObject[] GetTwoRectangles(ExportObject[] rectangles, Bar bar)
        {
            string id1 = bar.P1.X + "_" + bar.P1.Y;
            string id2 = bar.P2.X + "_" + bar.P2.Y;

            return new ExportObject[]
            {
                rectangles.Where(x => x.Bar == bar && x.AnchorPoints[0]?.Id == id1).First(),
                rectangles.Where(x => x.Bar == bar && x.AnchorPoints[1]?.Id == id2).First(),
            };
        }

        private static PrismaticJointExportData GetPrismaticJoint(IExportRigidBody[] allRigidBodys, RectangleExportData rec1, RectangleExportData rec2)
        {
            return new PrismaticJointExportData()
            {
                BodyIndex1 = allRigidBodys.IndexOf(rec1 as IExportRigidBody),
                BodyIndex2 = allRigidBodys.IndexOf(rec2 as IExportRigidBody),
                R1 = new Vec2D(rec1.Size.X / 2, 0),
                R2 = new Vec2D(-rec2.Size.X / 2, 0),
                MinTranslation = -1,
                MaxTranslation = 1,
                Motor = TranslationMotor.Disabled,
                SoftData = new SoftExportData() { ParameterType = SpringParameter.NoSoftness },
            };
        }

        #endregion

        #region Bridge+Ground
        private static PhysikLevelItemExportData GetBridgeAndGround(SimulatorInput input, string dataFolder, out ExportObject[] exportObjects)
        {
            var ground = GetGroundPolygon(input, dataFolder);            
            var circles = GetBridgeCircles(input, dataFolder, ground.AnchorPoints);
            var rectangles = GetBridgeRectangles(input);

            List<ExportObject> allObjects = new List<ExportObject>();
            allObjects.Add(ground);            
            allObjects.Add(circles);
            allObjects.Add(rectangles);

            var distanceJoints = GetDistanceJoints(allObjects.ToArray(), input.BridgeExport.Bars);
            var revoluteJoints = GetRevoluteJoints(allObjects.ToArray());
            var prismaticJoints = GetPrismaticJoints(allObjects.ToArray(), input.BridgeExport.Bars, input.LevelExport.GroundHeight);

            List<IExportJoint> joints = new List<IExportJoint>();
            joints.Add(revoluteJoints);
            joints.Add(distanceJoints);
            joints.Add(prismaticJoints);

            int levelItemId = 2;
            List<PhysicSceneTagdataEntry> tags = new List<PhysicSceneTagdataEntry>();
            var distanceJointTags = GetDistanceJointsTags(joints, distanceJoints, levelItemId, input.LevelExport.GroundHeight, input.BridgeExport.Bars);
            var rectangleTags = GetRectangleTags(allObjects.Select(x => x.ExportRigidBody).ToList(), rectangles, levelItemId);
            tags.Add(distanceJointTags);
            tags.Add(rectangleTags);

            exportObjects = allObjects.ToArray();

            return new PhysikLevelItemExportData()
            {
                LevelItemId = levelItemId,
                PhysicSceneData = new RigidBodyPhysics.ExportData.PhysicSceneExportData()
                {
                    Bodies = allObjects.Select(x => x.ExportRigidBody).ToArray(),
                    Joints = joints.ToArray(),
                },
                TextureData = new VisualisizerOutputData(allObjects.Select(x => x.TextureExportData).ToArray()),
                TagdataEntries = tags.ToArray(),
            };
        }
        #endregion
    }
}
