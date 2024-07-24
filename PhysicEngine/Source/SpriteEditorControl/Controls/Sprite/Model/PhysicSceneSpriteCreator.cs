using DynamicData.Aggregation;
using GraphicMinimal;
using GraphicPanels;
using KeyFrameEditorControl.Controls.KeyFrameEditor;
using KeyFrameGlobal;
using KeyFramePhysicImporter.Model;
using PhysicSceneDrawing;
using RigidBodyPhysics;
using RigidBodyPhysics.ExportData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TextureEditorGlobal;

namespace SpriteEditorControl.Controls.Sprite.Model
{
    //Wandelt ein AnimationOutputData-Objekt in ein Bitmap-Array um
    internal class PhysicSceneSpriteCreator
    {
        private Color ClearColor = Color.White;

        private AnimationOutputData animationData;
        private PhysicScene physicScene;
        private IAnimationProperty[] propertys;
        private FrameInterpolator frameInterpolator;
        private PhysicSceneDrawer drawer;

        class Frame
        {
            public Bitmap Image { get; set; }
            public RectangleF Box { get; set; }
        }

        public int PhysicSceneIterationCount { get => this.physicScene.IterationCount; set => this.physicScene.IterationCount = value; }

        public PhysicSceneSpriteCreator(PhysicSceneExportData physicSceneData, VisualisizerOutputData textureData, KeyFrameEditorExportData animationEditorData)
        {
            bool[] isFix = animationEditorData.ImporterData.IsFix;
            //Schritt 1: Wandle all die Bodys in Fix um, welche laut isFix so sein sollen
            physicSceneData = new PhysicScene(physicSceneData).GetExportData(); //Erzeuge eine Kopie vom Input um ihn nicht zu verändern
            for (int i = 0; i < isFix.Length; i++)
            {
                if (isFix[i])
                    physicSceneData.Bodies[i].MassData.Type = RigidBodyPhysics.ExportData.RigidBody.MassData.MassType.Infinity;
            }

            this.animationData = animationEditorData.AnimationData;
            this.physicScene = new PhysicScene(physicSceneData);
            this.physicScene.IterationCount = 100;
            this.propertys = PhysicSceneAnimationPropertyConverter.Convert(physicScene.GetAllPublicData()); //Hiermit wird die PhysicScene gesteuert
            this.frameInterpolator = new FrameInterpolator(propertys, animationData);

            this.drawer = new PhysicSceneDrawer(physicScene, textureData);            

            //Schritt 2: Springe zur Time-Position 0
            GoToTimePosition(0, 100);
        }

        //Verändert das PhysicScene-Objekt
        private void GoToTimePosition(float time, int timeStepsPerFrame)
        {
            var frame = frameInterpolator.GetFrame(time);
            propertys.WriteFrameToAnimatedObject(frame, animationData.PropertyIsAnimated); //Schreibe den interpolierten Frame auf das Animation-Anzeigeobjekt

            float timerIntercallInMilliseconds = 50;
            for (int i = 0; i < timeStepsPerFrame; i++)
            {
                physicScene.TimeStep(timerIntercallInMilliseconds);
            }
        }

        private RectangleF[] GetBoxs(int count, int timeStepsPerFrame)
        {
            List<RectangleF> boxes = new List<RectangleF>();
            for (int i = 0; i < count; i++)
            {
                GoToTimePosition(i / (float)(count - 1), timeStepsPerFrame);
                var box = drawer.GetTextureBoundingBoxFromScene();
                boxes.Add(box);
            }
            return boxes.ToArray();
        }

        private Frame GetFrame(GraphicPanel2D panel, Rectangle maxBox)
        {
            var box = drawer.GetTextureBoundingBoxFromScene();
            panel.ClearScreen(ClearColor);
            panel.MultTransformationMatrix(Matrix4x4.Translate(-maxBox.X, -maxBox.Y, 0)); //Die linke obere Ecke von der Max-Frame-Boundingbox soll bei (0,0) sein
            drawer.Draw(panel);
            panel.FlipBuffer();
            var screen = panel.GetScreenShoot();

            return new Frame()
            {
                Image = screen,
                Box = new RectangleF(box.X - maxBox.X, box.Y - maxBox.Y, box.Width, box.Height),
            };
        }

        //timeStepsPerFrame = So viele Frame läuft die Physiksimulation noch um das Model zu beruhigen
        private Frame[] GetFrames(GraphicPanel2D panel, int count, int timeStepsPerFrame, Rectangle maxBox)
        {           
            List< Frame > frames = new List<Frame>();
            for (int i=0; i<count; i++)
            {
                GoToTimePosition(i / (float)(count-1), timeStepsPerFrame);
                var frame = GetFrame(panel, maxBox);
                frames.Add(frame);
            }
            return frames.ToArray();
        }

        private RectangleF GetMaxBorder(IEnumerable<RectangleF> boxes)
        {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var box in boxes)
            {
                minX = Math.Min(minX, box.X);
                minY = Math.Min(minY, box.Y);
                maxX = Math.Max(maxX, box.Right);
                maxY = Math.Max(maxY, box.Bottom);
            }

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        private Rectangle RecFToRec(RectangleF rectF)
        {
            return new Rectangle((int)rectF.X, (int)rectF.Y, (int)(rectF.Width + 1), (int)(rectF.Height + 1));
        }

        
        //timeStepsPerFrame = So viele Timesteps bleibt das PhysicModel an einer Position, bevor der nächste Frame angesprungen wird
        public SpriteData GetSpriteImage(int count, SpriteExportData.PivotOriantation oriantation, int timeStepsPerFrame)
        {
            if (count < 2) throw new ArgumentException("The minimum value for count is 2");

            //Schritt 1: Ermittle von jeden Frame per PhysicSceneDrawer.GetTextureBoundingBoxFromScene wie groß es ist
            var boxes = GetBoxs(count, timeStepsPerFrame);
            var maxBox = RecFToRec(GetMaxBorder(boxes));        //Innerhalb dieser BoundinBox erscheinen alle Zeichendaten

            //Schritt 2: Erstelle alle Frames
            var panel = new GraphicPanel2D() { Width = maxBox.Width, Height = maxBox.Height, Mode = Mode2D.CPU };
            var frames = GetFrames(panel, count, timeStepsPerFrame, maxBox);

            int width = frames.Sum(x => x.Image.Width);
            int height = frames.Max(x => x.Image.Height);
            var image = new Bitmap(width, height);

            //Hintergrund löschen
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    image.SetPixel(x, y, ClearColor);
                }

            List<Rectangle> boxesOutput = new List<Rectangle>();
            int xStart = 0;
            foreach (var frame in frames)
            {
                //So viele Pixel wird das Einzelbild innerhalb des Sprite-Kästchens verschoben
                int xTrans = 0;
                int yTrans = 0;

                switch (oriantation)
                {
                    case SpriteExportData.PivotOriantation.Bottom:
                        yTrans += frame.Image.Height - (int)frame.Box.Bottom; //Verschiebe es auf die Bottom-Linie
                        break;
                }

                //Frame-Daten übertragen
                for (int x = (int)frame.Box.X; x < (int)frame.Box.Right; x++)
                    for (int y = (int)frame.Box.Y; y < (int)frame.Box.Bottom; y++)
                    {
                        image.SetPixel(xStart + x + xTrans, y + yTrans, frame.Image.GetPixel(x, y));                     
                    }

                var outputBox = new Rectangle((int)frame.Box.X + xTrans, (int)frame.Box.Y + yTrans, (int)frame.Box.Width, (int)frame.Box.Height);
                boxesOutput.Add(outputBox);

                //Testausgabe der Boundingbox
                // DrawBorder(image, new Rectangle(outputBox.X + xStart, outputBox.Y, outputBox.Width, outputBox.Height), Color.Blue);

                xStart += frame.Image.Width;
            }

            return new SpriteData() { Image = image, Boxes = boxesOutput.ToArray()};
        }

        private void DrawBorder(Bitmap image, Rectangle rect, Color color)
        {
            for (int x=rect.X;x<rect.Right;x++)
            {
                image.SetPixel(x, rect.Y, color);
                image.SetPixel(x, rect.Bottom - 1, color);
            }

            for (int y = rect.Y; y < rect.Bottom; y++)
            {
                image.SetPixel(rect.X, y, color);
                image.SetPixel(rect.Right - 1, y, color);
            }
        }
    }
}
