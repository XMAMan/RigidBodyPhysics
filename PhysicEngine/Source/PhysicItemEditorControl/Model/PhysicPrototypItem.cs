using GraphicPanels;
using KeyFrameGlobal;
using KeyFramePhysicImporter.Model;
using PhysicSceneDrawing;
using RigidBodyPhysics.ExportData;
using RigidBodyPhysics;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TextureEditorControl;
using TextureEditorGlobal;
using WpfControls.Controls.CameraSetting;
using LevelEditorGlobal;
using GraphicMinimal;
using PhysicSceneKeyboardControl;
using PhysicItemEditorControl.Model.MouseClickable;
using static LevelEditorGlobal.ITagable;

namespace PhysicItemEditorControl.Model
{
    internal class PhysicPrototypItem : IPrototypItem, IKeyboardControlledLevelItem, ICollidableContainer, ITagableContainer, ITagable
    {
        private PhysicScene physicScene;
        private PhysicSceneDrawer sceneDrawer = null;
        public IPrototypExportData EditorExportData { get; set; }

        public PhysicPrototypItem(PhysicItemExportData data)
        {
            this.EditorExportData = data;

            this.Id = data.Id;
            this.InitialRecValues = data.InitialRecValues;

            if (data.PhysicSceneForAnimationNull != null)
                this.physicScene = new PhysicScene((PhysicSceneExportData)data.PhysicSceneForAnimationNull);
            else
                this.physicScene = new PhysicScene((PhysicSceneExportData)data.PhysicSceneData);

            if (data.TextureData != null)
            {
                var textureData = (VisualisizerOutputData)data.TextureData;

                this.sceneDrawer = new PhysicSceneDrawer(physicScene, textureData);
            }
            else if (physicScene.GetAllBodys().Any())
            {
                //Nutze leere Texturen, wenn nichts bis jetzt angelegt wurde
                var textureData = TextureEditorFactory.CreateDefaultTextureData(data.PhysicSceneData);
                this.sceneDrawer = new PhysicSceneDrawer(physicScene, textureData);
            }

            this.BoundingBox = this.sceneDrawer.GetPhysicBoundingBoxFromScene();


            //Erzeuge die MouseClickable Objekte
            List<MouseClickableBody> mouseClickBodys = new List<MouseClickableBody>();
            var runtimeBodys = this.physicScene.GetAllBodys();
            for (int i = 0; i < data.PhysicSceneData.Bodies.Length; i++)
            {
                mouseClickBodys.Add(new MouseClickableBody(data.PhysicSceneData.Bodies[i], runtimeBodys[i], this.BoundingBox, i));
            }

            List<MouseClickableJoints> mouseClickJoints = new List<MouseClickableJoints>();
            var runtimeJoints = this.physicScene.GetAllJoints();
            for (int i=0;i<runtimeJoints.Length;i++)
            {
                mouseClickJoints.Add(new MouseClickableJoints(runtimeJoints[i], this.BoundingBox, i));
            }

            List<MouseClickableThruster> mouseClickableThrusters = new List<MouseClickableThruster>();
            var runtimeThrusters = this.physicScene.GetAllThrusters();
            for (int i=0;i<runtimeThrusters.Length;i++)
            {
                mouseClickableThrusters.Add(new MouseClickableThruster(runtimeThrusters[i], this.BoundingBox, i));  
            }

            List<MouseClickableRotaryMotor> mouseClickableMotors = new List<MouseClickableRotaryMotor>();
            var runtimeMotors = this.physicScene.GetAllRotaryMotors();
            for (int i = 0; i < runtimeMotors.Length; i++)
            {
                mouseClickableMotors.Add(new MouseClickableRotaryMotor(runtimeMotors[i], this.BoundingBox, i));
            }

            List<MouseClickableAxialFriction> mouseClickableAxialFrictions = new List<MouseClickableAxialFriction>();
            var runtimeAxialFrictions = this.physicScene.GetAllAxialFrictions();
            for (int i = 0; i < runtimeAxialFrictions.Length; i++)
            {
                mouseClickableAxialFrictions.Add(new MouseClickableAxialFriction(runtimeAxialFrictions[i], this.BoundingBox, i));
            }

            List<IMouseclickableWithTagData> mouseClickable = new List<IMouseclickableWithTagData>();
            mouseClickable.AddRange(mouseClickBodys);
            mouseClickable.AddRange(mouseClickJoints);
            mouseClickable.AddRange(mouseClickableThrusters);
            mouseClickable.AddRange(mouseClickableMotors);
            mouseClickable.AddRange(mouseClickableAxialFrictions);

            this.Collidables = mouseClickBodys.ToArray();
            this.Tagables = mouseClickable.ToArray();
        }

        public IPrototypItem.Type ProtoType { get => IPrototypItem.Type.PhysicItem; }
        public int Id { get; }
        public TagType TypeName { get => TagType.Proto; } //ITagable
        public RectangleF BoundingBox { get; }
        public InitialRotatedRectangleValues InitialRecValues { get; }

        public ICollidable[] Collidables { get; }
        public IMouseclickableWithTagData[] Tagables { get; }
        public Bitmap GetImage(int maxWidth, int maxHeight)
        {
            var panel = new GraphicPanel2D() { Width = maxWidth, Height = maxHeight, Mode = Mode2D.CPU };
            var camera = new Camera2D(maxWidth, maxHeight, this.BoundingBox) { ShowOriginalPosition = false };

            panel.ClearScreen(Color.White);
            panel.MultTransformationMatrix(camera.GetPointToSceenMatrix());
            this.sceneDrawer.Draw(panel);
            panel.FlipBuffer();

            return panel.GetScreenShoot();
        }


        public void Draw(GraphicPanel2D panel)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-this.BoundingBox.X, -this.BoundingBox.Y, 0));

            this.sceneDrawer.Draw(panel);
            panel.PopMatrix();
        }

        public void DrawBorder(GraphicPanel2D panel, Pen borderPen)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-this.BoundingBox.X, -this.BoundingBox.Y, 0));
            this.sceneDrawer.DrawPhysicBorder(panel, borderPen);
            this.sceneDrawer.DrawTextureBorder(panel, Pens.Green);
            panel.PopMatrix();
        }
        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            panel.PushMatrix();
            panel.MultTransformationMatrix(Matrix4x4.Translate(-this.BoundingBox.X, -this.BoundingBox.Y, 0));
            this.sceneDrawer.DrawWithTwoColors(panel, frontColor, backColor);
            panel.PopMatrix();
        }

        public string[] GetAllKeyPressHandlerNames()
        {
            var physicObjects = this.physicScene.GetAllPublicData();
            Animator[] animators = null;

            var animationExport = (PhysicItemExportData)EditorExportData;           
            if (animationExport.AnimationData != null)
            {
                var animations = animationExport.AnimationData.Select(x => x.AnimationData).ToArray();
                var animatedPhysicScene = new AnimatedPhysicObjects(physicObjects, animations, 50);
                animators = animatedPhysicScene.Animators;
            }

            return PhysicSceneKeyPressHandlerProvider.GetHandler(physicObjects, animators).Select(x => x.KeyPressDescription).ToArray();
        }
    }
}
