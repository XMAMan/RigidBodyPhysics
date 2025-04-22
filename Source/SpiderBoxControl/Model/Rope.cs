using DynamicData;
using GameHelper.Simulation;
using GraphicMinimal;
using GraphicPanels;
using LevelEditorGlobal;
using RigidBodyPhysics;
using RigidBodyPhysics.CollisionDetection;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics.ExportData.Joints;
using RigidBodyPhysics.ExportData.RigidBody;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.RuntimeObjects.Joints;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TextureEditorGlobal;

namespace SpiderBoxControl.Model
{
    //Seil, was aus lauter Kreisen besteht, die per Distanzjoints verbunden sind
    class Rope
    {
        private GameSimulator simulator;
        private int ropeLevelItem;
        private IPublicRigidBody lastCircle; //Wird benötigt um zu sehen, ob das Seil gegen den Boden stößt
        private IPublicDistanceJoint[] distanceJoints; //Alle Segmente vom Seil -> Zur Anzeige des Seils
        private IPublicDistanceJoint clampFirst; //Verbindet firstCircle mit dem arm

        private int clampLevelItem;
        private IPublicDistanceJoint clampLast; //Verbindet lastCircle mit dem Boden, wenn es zur Kollision kommt

        private int circleCount = 10;
        private float circleRadius = 5;
        private float circleDensity = 0.007f;
        public const float MinSegmentLength = 10;
        public const float MaxSegmentLength = 100;

        private SoftExportData softData = new SoftExportData()
        {
            ParameterType = IPublicJoint.SpringParameter.StiffnessAndDamping,
            Damping = 1.0f,
            Stiffness = 2000,
        };

        private float segmentLength = 50; //Sollwertlänge für die Seilsegmente
        public float SegmentLength
        {
            get
            {
                return this.segmentLength;
            }
            set
            {
                this.segmentLength = value;

                foreach (var joint in this.distanceJoints) 
                    joint.LengthPosition = this.segmentLength;

                this.clampFirst.LengthPosition = this.segmentLength;

                if (this.clampLast != null)
                {
                    //this.clampLast.LengthPosition = this.segmentLength;
                }
            }
        }

        private static Vec2D GetImpulseFromRobe(PhysikLevelItemExportData export)
        {
            var circles = export.PhysicSceneData.Bodies.Cast<CircleExportData>().ToArray();

            float sum = 0;
            foreach (var c in circles)
            {
                float area = c.Radius * c.Radius * (float)Math.PI;
                float mass = c.MassData.GetMass(area);
                float impulse = c.Velocity.Length() * mass;
                sum += impulse;
            }

            return circles.First().Velocity.Normalize() * sum;
        }

        //Erzeugt an bei position 'circleCount' Kreise, welche mit einer initialen Geschindigkeit von initialVelocity fliegen
        public Rope(IPublicRigidBody arm, Vec2D position, Vec2D initialVelocity, GameSimulator simulator, float timerIntervallInMilliseconds)
        {
            this.simulator = simulator;

            //Seil zur Simulation hinzufügen
            var ropeExportData = CreateRopeExportData(position, initialVelocity);
            this.ropeLevelItem = simulator.AddLevelItem(ropeExportData);
            
            //Seil mit dem Arm verknüpfen
            var firstCircle = simulator.GetBodyByTagName(this.ropeLevelItem, "FirstCircleRope");
            var r1 = PhysicScene.GetLocalDirectionFromWorldPoint(arm, position);
            int index1 = this.simulator.GetAllBodiesOfType<IPublicRigidBody>().IndexOf(arm);
            var r2 = new Vec2D(0, 0);
            int index2 = this.simulator.GetAllBodiesOfType<IPublicRigidBody>().IndexOf(firstCircle);
            this.clampFirst = (IPublicDistanceJoint)simulator.AddJoint(new DistanceJointExportData()
            {
                BodyIndex1 = index1,
                BodyIndex2 = index2,
                R1 = r1,
                R2 = r2,
                CollideConnected = false,
                LimitIsEnabled = true,
                JointIsRope = true,
                MaxLength = Rope.MaxSegmentLength,
                SoftData = this.softData,
            });

            //Ermittle Visualisierungsdaten
            var distanceJointsList = simulator.GetJointsByTagName(this.ropeLevelItem, "DistanceRope").Cast<IPublicDistanceJoint>().ToList();
            this.distanceJoints = distanceJointsList.ToArray();

            //Seil-Boden-Kollisions
            this.lastCircle = simulator.GetBodyByTagName(this.ropeLevelItem, "LastCircleRope");
            simulator.CollisonOccured += Simulator_CollisonOccured;

            this.SegmentLength = 50;

            //Rückstoß an den Arm geben
            Vec2D impulse = -GetImpulseFromRobe(ropeExportData) * 0.3f;
            arm.Force = impulse / timerIntervallInMilliseconds;
        }

        private void Simulator_CollisonOccured(RigidBodyPhysics.PhysicScene sender, RigidBodyPhysics.CollisionDetection.PublicRigidBodyCollision[] collisions)
        {
            if (this.clampLast != null) return;

            foreach (var collision in collisions)
            {
                if (collision.Body2 == this.lastCircle)
                {
                    this.clampLast = (IPublicDistanceJoint)simulator.AddJoint(CreateClampExportData(collision));
                    this.clampLast.LengthPosition = this.circleRadius * 3;
                }
            }
        }

        private PhysikLevelItemExportData CreateRopeExportData(Vec2D position, Vec2D initialVelocity)
        {
            List<IExportRigidBody> circles = new List<IExportRigidBody>();
            List<TextureExportData> textures = new List<TextureExportData>();
            List<PhysicSceneTagdataEntry> tags = new List<PhysicSceneTagdataEntry>();

            for (int i = 0; i < circleCount; i++)
            {
                var circleData = new CircleExportData()
                {
                    Radius = circleRadius,
                    Center = position,
                    Velocity = initialVelocity,
                    MassData = new MassData(MassData.MassType.Density, 1, this.circleDensity),
                    AngleInDegree = 0,
                    AngularVelocity = 0,
                    Friction = 0,
                    Restituion = 0.2f,
                    CollisionCategory = 2,//0=Boden, 1=Tonne, 2 = Seil
                };
                circles.Add(circleData);

                var texture = new TextureExportData()
                {
                    TextureFile = null,
                    DeltaX = 0,
                    DeltaY = 0,
                    DeltaAngle = 0,
                    Width = circleRadius * 2,
                    Height = circleRadius * 2,
                    ZValue = 0,
                    IsInvisible = true,
                };
                textures.Add(texture);
            }

            List<IExportJoint> distanceJoints = new List<IExportJoint>();
            for (int i = 0; i < circleCount - 1; i++)
            {
                var distanceJoint = new DistanceJointExportData()
                {
                    BodyIndex1 = i,
                    BodyIndex2 = i + 1,
                    R1 = new Vec2D(0, 0),
                    R2 = new Vec2D(0, 0),
                    CollideConnected = false,
                    LimitIsEnabled = true,
                    JointIsRope = true,
                    MaxLength = Rope.MaxSegmentLength,
                    SoftData = this.softData,
                };
                distanceJoints.Add(distanceJoint);

                tags.Add(new PhysicSceneTagdataEntry(ITagable.TagType.Joint, -1, i, new string[] { "DistanceRope" }, 0, new Vector2D[0]));
            }

            tags.Add(new PhysicSceneTagdataEntry(ITagable.TagType.Body, -1, 0, new string[] { "FirstCircleRope" }, 0, new Vector2D[0]));
            tags.Add(new PhysicSceneTagdataEntry(ITagable.TagType.Body, -1, circles.Count - 1, new string[] { "LastCircleRope" }, 0, new Vector2D[0]));

            var levelItemData = new PhysikLevelItemExportData()
            {
                PhysicSceneData = new RigidBodyPhysics.ExportData.PhysicSceneExportData()
                {
                    Bodies = circles.ToArray(),
                    Joints = distanceJoints.ToArray(),
                },
                TextureData = new VisualisizerOutputData(textures.ToArray()),
                TagdataEntries = tags.ToArray(),
            };

            return levelItemData;
        }

        private DistanceJointExportData CreateClampExportData(PublicRigidBodyCollision collision)
        {
            int index1 = this.simulator.GetAllBodiesOfType<IPublicRigidBody>().IndexOf(collision.Body1); //Boden
            int index2 = this.simulator.GetAllBodiesOfType<IPublicRigidBody>().IndexOf(collision.Body2); //lastCircle
            var r1 = PhysicScene.GetLocalDirectionFromWorldPoint(collision.Body1, collision.End);

            var distanceJoint = new DistanceJointExportData()
            {
                BodyIndex1 = index1,
                BodyIndex2 = index2,
                R1 = r1,
                R2 = new Vec2D(0,0),
                CollideConnected = false,
                LimitIsEnabled = true,
                JointIsRope = true,
                MaxLength = Rope.MaxSegmentLength,
                SoftData = this.softData,
            };

            return distanceJoint;
        }

        public void Draw(GraphicPanel2D panel)
        {
            Pen pen = new Pen(Color.Black, 3);
            foreach (var d in this.distanceJoints)
            {
                 panel.DrawLine(pen, d.Anchor1.ToGrx(), d.Anchor2.ToGrx());
            }

            {
                var d = this.clampFirst;
                panel.DrawLine(pen, d.Anchor1.ToGrx(), d.Anchor2.ToGrx());
            }

            if (this.clampLast != null)
            {
                var d = this.clampLast;
                panel.DrawLine(pen, d.Anchor1.ToGrx(), d.Anchor2.ToGrx());                
            }

            if (this.clampLast == null)
                panel.DrawFillCircle(Color.Black, distanceJoints.Last().Anchor2.ToGrx(), 3);
            else
                panel.DrawFillCircle(Color.Black, clampLast.Anchor1.ToGrx(), 3);
        }

        public void RemoveFromSimulation()
        {
            this.simulator.RemoveJoint(this.clampFirst);

            if (this.clampLast != null)
                this.simulator.RemoveJoint(this.clampLast);

            this.simulator.RemoveLevelItem(this.ropeLevelItem);
        }

        public void IncreaseLength()
        {
            this.SegmentLength = Math.Min(Rope.MaxSegmentLength, this.SegmentLength + 1.1f);
        }

        public void DecreaseLength()
        {
            this.SegmentLength = Math.Max(Rope.MinSegmentLength, this.SegmentLength - 1.1f);
        }
    }
}
