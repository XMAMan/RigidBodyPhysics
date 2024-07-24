using ControlInterfaces;
using GraphicPanels;
using JsonHelper;
using PhysicEngine.ExportData;
using PhysicEngine.Joints;
using PhysicEngine.MathHelper;
using PhysicEngine.MouseBodyClick;
using PhysicEngine.RigidBody;
using PhysicEngine;
using SimulatorControl.Model.ShapeExporter;
using SimulatorControl.Model.SimulatorJoint;
using SimulatorControl.Model.SimulatorShape;

namespace SimulatorControl.Model
{
    //Enthält Menge von Rechtecken und Kreisen
    internal class Scene : IShapeDataContainer
    {
        private PhysicScene scene = new PhysicScene();
        private List<ISimulatorShape> shapes = new List<ISimulatorShape>();
        private List<ISimulatorJoint> joints = new List<ISimulatorJoint>();

        public bool ShowCollisionPoints { get; set; } = false;
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

        public string BackgroundImage = "#FFFFFF";


        //Aktualisiert die Position der Objekte
        //dt = So viele Millisekunden sind vergangen
        public void TimeStep(float dt)
        {
            this.scene.TimeStep(dt);
        }

        public void Draw(GraphicPanel2D panel)
        {
            panel.ClearScreen(this.BackgroundImage);
            //panel.DrawLine(Pens.Black, new GraphicMinimal.Vector2D(0, 0), new GraphicMinimal.Vector2D(panel.Width, panel.Height));

            foreach (ISimulatorShape shape in this.shapes) { shape.Draw(panel); }
            foreach (ISimulatorJoint joint in this.joints) { joint.Draw(panel); }

            if (this.ShowCollisionPoints)
            {
                foreach (var c in this.scene.GetCollisions())
                {
                    panel.DrawFillCircle(Color.Green, c.Start.ToGrx(), 3);
                    panel.DrawFillCircle(Color.Blue, c.End.ToGrx(), 3);
                    panel.DrawLine(Pens.Red, c.Start.ToGrx(), (c.Start + c.Normal * 20).ToGrx());
                }
            }

            panel.FlipBuffer();
        }

        public string GetShapeData()
        {
            return ExportHelper.ToJson(this.scene.GetExportData());
        }

        public void LoadShapeData(string json)
        {
            var rawData = Helper.FromCompactJson<PhysicSceneExportData>(json);
            this.scene.Reload(rawData);

            this.shapes = new List<ISimulatorShape>();
            foreach (var ctor in this.scene.GetAllBodys())
            {
                if (ctor is IPublicRigidRectangle)
                    shapes.Add(new SimulatorRectangle((ctor as IPublicRigidRectangle)));

                if (ctor is IPublicRigidCircle)
                    shapes.Add(new SimulatorCircle((ctor as IPublicRigidCircle)));
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
        }

        public void PushBodysApart()
        {
            this.scene.PushBodysApart();
        }

        public IPublicJoint[] GetAllJoints()
        {
            return this.scene.GetAllJoints();
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
