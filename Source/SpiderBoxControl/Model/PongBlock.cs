using GameHelper.Simulation;
using GraphicPanels;
using KeyboardRecordAndPlay;
using PhysicGlobal;
using PhysicSceneDrawing;
using RigidBodyPhysics.RuntimeObjects.RigidBody;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace SpiderBoxControl.Model
{
    //Anstelle einer Sprite-Animation wird hier eine Physiksimulation innerhalb einer Box angezeigt
    internal class PongBlock : IRigidBodyDrawer
    {
        private IPublicRigidRectangle block;
        private GameSimulator pongSimulator;
        private PhysicGlobal.BoundingBox sceneBoundingBox;
        private KeyBoardPlayer keyBoardPlayer = null;

        public PongBlock(GameSimulator simulator, string dataFolder, float timerIntervallInMilliseconds)
        {
            this.block = (IPublicRigidRectangle)simulator.GetBodyByTagName("PongBlock");
            this.pongSimulator = new GameSimulator(dataFolder + "PongLevel.txt", new Size((int)block.Size.X, (int)block.Size.Y), timerIntervallInMilliseconds);
            this.sceneBoundingBox = this.pongSimulator.GetBoundingBoxFromScene();

            simulator.UseCustomDrawingForRigidBody(this.block, this);

            var recordData = JsonHelper.Helper.CreateFromJson<KeyBoardRecordData>(File.ReadAllText(dataFolder + "PongReplay.txt"));
            this.keyBoardPlayer = new KeyBoardPlayer(new KeyBoardPlayerConstructorData()
            {
                RecordData = recordData,
                KeyDownAction = (key) => { this.pongSimulator.HandleKeyDown(key); },
                KeyUpAction = (key) => { this.pongSimulator.HandleKeyUp(key); },
                IsFinish = () => { }
            });
        }

        public void Draw(GraphicPanel2D panel)
        {
            float sizeX = this.block.Size.X / this.sceneBoundingBox.GetWidth();
            float sizeY = this.block.Size.Y / this.sceneBoundingBox.GetHeight();
            float size = Math.Min(sizeX, sizeY);

            //Zeichne das Pong-Level innerhalb des Blocks
            var m = PhxMatrix.Ident();
            m *= PhxMatrix.Translate(-this.sceneBoundingBox.GetCenter().X, -this.sceneBoundingBox.GetCenter().Y, 0);
            m *= PhxMatrix.Scale(size, size, size);
            m *= PhxMatrix.Rotate(block.Angle / (float)Math.PI * 180, 0, 0, 1);
            m *= PhxMatrix.Translate(this.block.Center.X, this.block.Center.Y, 0);            
           
            //Hintergrund zeichnen
            panel.DrawFillRectangle(Color.Black, block.Center.X, block.Center.Y, block.Size.X, block.Size.Y, block.Angle / (float)Math.PI * 180);
            panel.DrawPolygon(new Pen(Color.Red, 2), block.Vertex.Select(x => x.ToGrx()).ToList());

            //Physikitems zeichnen
            panel.PushMatrix();
            panel.MultTransformationMatrix(m.To4x4Matrix());
            this.pongSimulator.DrawPhysicItems(panel, false);
            panel.PopMatrix();
        }

        public void DrawWithTwoColors(GraphicPanel2D panel, Color frontColor, Color backColor)
        {
            panel.DrawFillRectangle(frontColor, block.Center.X, block.Center.Y, block.Size.X, block.Size.Y, block.Angle / (float)Math.PI * 180);
        }

        public void MoveOneStep(float dt)
        {
            this.keyBoardPlayer.HandleTimerTick(dt);
            if (this.keyBoardPlayer.IsFinish)
            {
                this.keyBoardPlayer.Reset();
            }
            this.pongSimulator.MoveOneStep(dt);
        }
    }
}
