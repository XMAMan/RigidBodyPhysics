using GraphicPanels;
using JsonHelper;
using PhysicSceneSimulatorControl.Controls.Simulator.Model.ShapeExporter;
using PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorJoint;
using PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorRotaryMotor;
using PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorShape;
using PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorThruster;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics.MouseBodyClick;
using RigidBodyPhysics;
using WpfControls.Model;
using PhysicSceneSimulatorControl.Dialogs.PrintSettings;
using PhysicSceneSimulatorControl.Controls.Simulator.Model.SimulatorAxialFriction;
using RigidBodyPhysics.RuntimeObjects.Thruster;
using RigidBodyPhysics.RuntimeObjects.RotaryMotor;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using RigidBodyPhysics.RuntimeObjects.Joints;

namespace PhysicSceneSimulatorControl.Controls.Simulator.Model
{
    //Enthält Menge von Rechtecken und Kreisen
    internal class Scene : IStringSerializable, IObjectSerializable
    {
        private PhysicScene scene = new PhysicScene();
        private List<ISimulatorShape> shapes = new List<ISimulatorShape>();
        private List<ISimulatorJoint> joints = new List<ISimulatorJoint>();
        private List<ISimulatorThruster> thrusters = new List<ISimulatorThruster>();
        private List<ISimulatorRotaryMotor> motors = new List<ISimulatorRotaryMotor>();
        private List<ISimulatorAxialFriction> axialFrictions = new List<ISimulatorAxialFriction>();

        public bool DoPositionalCorrection
        {
            get => this.scene.DoPositionalCorrection;
            set => this.scene.DoPositionalCorrection = value;
        }
        public bool DoWarmStart
        {
            get => this.scene.DoWarmStart;
            set => this.scene.DoWarmStart = value;
        }
        public bool HasGravity
        {
            get => this.scene.HasGravity;
            set => this.scene.HasGravity = value;
        }

        public PhysicScene.SolverType SolverType
        {
            get => this.scene.Solver;
            set => this.scene.Solver = value;
        }

        public int IterationCount
        {
            get => this.scene.IterationCount;
            set => this.scene.IterationCount = value;
        }

        //Aktualisiert die Position der Objekte
        //dt = So viele Millisekunden sind vergangen
        public void TimeStep(float dt)
        {
            this.scene.TimeStep(dt);
        }

        public void Draw(GraphicPanel2D panel, PrintSettingsViewModel printSettings)
        {
            //panel.DrawLine(Pens.Black, new GraphicMinimal.Vector2D(0, 0), new GraphicMinimal.Vector2D(panel.Width, panel.Height));

            foreach (ISimulatorShape shape in this.shapes)
            {
                shape.Draw(panel, printSettings);

                if (printSettings.ShowOrientation)
                    panel.DrawString(shape.PhysicModel.Center.ToGrx(), Color.Black, 20, (int)(shape.PhysicModel.Angle / (2 * Math.PI) * 360) + "");

                if (printSettings.ShowBodyIndex)
                    panel.DrawString(shape.PhysicModel.Center.ToGrx(), Color.Black, 20, this.shapes.IndexOf(shape) + "");

                if (printSettings.ShowPushPullForce && shape.PhysicModel is IPublicRigidRectangle)
                    panel.DrawString(shape.PhysicModel.Center.ToGrx(), Color.Black, 20, (shape.PhysicModel as IPublicRigidRectangle).GetPushPullForce() + "");
            }

            foreach (ISimulatorJoint joint in this.joints) joint.Draw(panel, printSettings);
            foreach (ISimulatorThruster thruster in this.thrusters) thruster.Draw(panel, printSettings);
            foreach (ISimulatorRotaryMotor motor in this.motors) motor.Draw(panel, printSettings);
            foreach (ISimulatorAxialFriction axialFriction in this.axialFrictions) axialFriction.Draw(panel, printSettings);

            if (printSettings.ShowCollisionPoints)
            {
                foreach (var c in this.scene.GetCollisions())
                {
                    panel.DrawFillCircle(Color.Green, c.Start.ToGrx(), 3);
                    panel.DrawFillCircle(Color.Blue, c.End.ToGrx(), 3);
                    panel.DrawLine(Pens.Red, c.Start.ToGrx(), (c.Start + c.Normal * 20).ToGrx());
                }
            }
        }

        public object GetExportObject()
        {
            return this.scene.GetExportData();
        }
        public void LoadFromExportObject(object exportObject)
        {
            PhysicSceneExportData rawData = (PhysicSceneExportData)exportObject;
            this.scene.Reload(rawData); //Ich darf hier nicht "this.scene = new PhysicScene(rawData);" schreiben, da sonst all die Bool-Flags (HasGravity, DoPositionCorretion) vom PhysicScene-Objekt ihren Wert verlieren

            this.shapes = new List<ISimulatorShape>();
            foreach (var ctor in this.scene.GetAllBodys())
            {
                if (ctor is IPublicRigidRectangle)
                    shapes.Add(new SimulatorRectangle((ctor as IPublicRigidRectangle)));

                if (ctor is IPublicRigidCircle)
                    shapes.Add(new SimulatorCircle((ctor as IPublicRigidCircle)));

                if (ctor is IPublicRigidPolygon)
                    shapes.Add(new SimulatorPolygon((ctor as IPublicRigidPolygon)));
            }

            this.joints = new List<ISimulatorJoint>();
            foreach (var ctor in this.scene.GetAllJoints())
            {
                if (ctor is IPublicDistanceJoint)
                    joints.Add(new SimulatorDistanceJoint(ctor as IPublicDistanceJoint));

                if (ctor is IPublicRevoluteJoint)
                    joints.Add(new SimulatorRevoluteJoint(ctor as IPublicRevoluteJoint));

                if (ctor is IPublicPrismaticJoint)
                    joints.Add(new SimulatorPrismaticJoint(ctor as IPublicPrismaticJoint));

                if (ctor is IPublicWeldJoint)
                    joints.Add(new SimulatorWeldJoint(ctor as IPublicWeldJoint));

                if (ctor is IPublicWheelJoint)
                    joints.Add(new SimulatorWheelJoint(ctor as IPublicWheelJoint));
            }

            this.thrusters = new List<ISimulatorThruster>();
            foreach (var ctor in this.scene.GetAllThrusters())
            {
                thrusters.Add(new SimulatorThruster.SimulatorThruster(ctor));
            }

            this.motors = new List<ISimulatorRotaryMotor>();
            foreach (var ctor in this.scene.GetAllRotaryMotors())
            {
                motors.Add(new SimulatorRotaryMotor.SimulatorRotaryMotor(ctor));
            }

            this.axialFrictions = new List<ISimulatorAxialFriction>();
            foreach (var ctor in this.scene.GetAllAxialFrictions())
            {
                axialFrictions.Add(new SimulatorAxialFriction.SimulatorAxialFriction(ctor));
            }
        }

        public string GetExportString()
        {
            return ExportHelper.ToJson(this.scene.GetExportData());
        }

        public void LoadFromExportString(string json)
        {
            var rawData = Helper.FromCompactJson<PhysicSceneExportData>(json);
            LoadFromExportObject(rawData);
        }

        public void PushBodysApart()
        {
            this.scene.PushBodysApart();
        }

        public IPublicJoint[] GetAllJoints()
        {
            return this.scene.GetAllJoints();
        }

        public IPublicThruster[] GetAllThrusters()
        {
            return this.scene.GetAllThrusters();
        }

        public IPublicRotaryMotor[] GetAllRotaryMotors()
        {
            return this.scene.GetAllRotaryMotors();
        }

        public string GetStateFromAllBodies()
        {
            return string.Join(";", this.shapes.Select(x => BodyToString(x.PhysicModel)));
        }

        private static string BodyToString(IPublicRigidBody body)
        {
            return "[" +
                FloatToString(body.Center.X) + " " + FloatToString(body.Center.Y) + FloatToString(body.Angle) +
                FloatToString(body.Velocity.X) + " " + FloatToString(body.Velocity.Y) + FloatToString(body.AngularVelocity) + "]";
        }

        private static string FloatToString(float x)
        {
            return x.ToString("G9");
            //return String.Format("{0:+0.00000;-0.00000; 0.00000}", x);
        }

        public MouseClickData TryToGetBodyWithMouseClick(Vec2D mousePosition)
        {
            return this.scene.TryToGetBodyWithMouseClick(mousePosition);
        }

        public void SetMouseConstraint(MouseClickData mouseClick, MouseConstraintUserData userData)
        {
            this.scene.SetMouseConstraint(mouseClick, userData);
        }

        public void ClearMouseConstraint()
        {
            this.scene.ClearMouseConstraint();
        }

        public void UpdateMousePosition(Vec2D mousePosition)
        {
            this.scene.UpdateMousePosition(mousePosition);
        }
    }
}
