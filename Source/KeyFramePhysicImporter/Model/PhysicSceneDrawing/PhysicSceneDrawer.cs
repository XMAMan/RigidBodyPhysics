using GraphicPanels;
using KeyFrameGlobal;
using RigidBodyPhysics.MathHelper;
using RigidBodyPhysics;
using WpfControls.Controls.CameraSetting;
using RigidBodyPhysics.RuntimeObjects.RigidBody;

namespace KeyFramePhysicImporter.Model.PhysicSceneDrawing
{
    internal class PhysicSceneDrawer : IAnimationModelDrawer
    {
        private GraphicPanel2D panel;
        private PhysicScene physicScene;
        private IShape[] shapes;
        private bool[] isFix;

        public PhysicSceneDrawer(PhysicScene physicScene, GraphicPanel2D panel)
        {
            this.panel = panel;
            ReloadNewPhysicScene(physicScene);

        }

        public Color[] GetColors()
        {
            return this.shapes.Select(x => x.FillColor).ToArray();
        }

        public void SetColorFromRigidBody(IPublicRigidBody body, Color color)
        {
            this.shapes.First(x => x.PhysicModel == body).FillColor = color;
        }

        private static IShape[] GetAllShapes(PhysicScene physicScene)
        {
            List<IShape> shapes = new List<IShape>();
            foreach (var ctor in physicScene.GetAllBodys())
            {
                if (ctor is IPublicRigidRectangle)
                    shapes.Add(new RectangleShape((ctor as IPublicRigidRectangle)));

                if (ctor is IPublicRigidCircle)
                    shapes.Add(new CircleShape((ctor as IPublicRigidCircle)));

                if (ctor is IPublicRigidPolygon)
                    shapes.Add(new PolygonShape((ctor as IPublicRigidPolygon)));
            }

            return shapes.ToArray();
        }

        //Muss aufgerufen werden, wenn der IsFix-Zustand geändert wurde (Weil diese Property Readonly ist muss eine erneute Scene erstellt werden)
        public void ReloadNewPhysicScene(PhysicScene physicScene)
        {
            this.physicScene = physicScene;

            var colors = this.shapes != null ? this.shapes.Select(x => x.FillColor).ToArray() : null;
            this.shapes = GetAllShapes(this.physicScene);
            this.isFix = this.physicScene.GetExportData().Bodies.Select(x => x.MassData.Type == RigidBodyPhysics.ExportData.RigidBody.MassData.MassType.Infinity).ToArray();

            if (colors != null)
            {
                for (int i = 0; i < this.shapes.Length; i++)
                {
                    this.shapes[i].FillColor = colors[i];
                }
            }
        }

        #region IAnimationModelDrawer
        public RectangleF GetBoundingBoxFromScene()
        {
            return BoundingBox.GetBoxFromBoxes(this.shapes.Select(x => x.BoundingBox)).ToRectangleF();
        }
        public void Draw(Camera2D camera)
        {
            for (int i = 0; i < this.shapes.Length; i++)
            {
                var shape = this.shapes[i];
                shape.Draw(panel, this.isFix[i] ? Pens.Red : Pens.Black, shape.FillColor, camera);
            }

            //Die Nummer von jeden Gelenk ausgeben
            var joints = this.physicScene.GetAllJoints();
            for (int i = 0; i < joints.Length; i++)
            {
                var joint = joints[i];

                var center = joint.Anchor1;
                var b1 = joint.Body1.Center;
                var b2 = joint.Body2.Center;
                var m = 0.5f * b1 + 0.5f * b2;
                float length = (m - center).Length();
                Vec2D dir = new Vec2D(1, 0);
                if (length > 0.1f)
                {
                    dir = (m - center).Normalize();
                }
                panel.DrawStringOnCircleBorder(i + "", 20, Color.Red, camera.PointToScreen(center.ToPointF()).ToPhx(), dir);
            }
        }
        #endregion



    }
}
