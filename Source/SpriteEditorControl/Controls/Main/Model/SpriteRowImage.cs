using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;
using SpriteEditorControl.Controls.Sprite.Model;
using System;
using System.Drawing;

namespace SpriteEditorControl.Controls.Main.Model
{
    internal class SpriteRowImage : ITimerHandler
    {
        private string textureName;
        private int xCount;
        private float durrationInMs;
        private float time = 0;
        private int xPos = 0;
        private SpriteData spriteData;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Bitmap Image { get => this.spriteData.Image; }

        public SpriteRowImage(string textureName, SpriteData spriteData, float durrationInSeconds)
        {
            this.spriteData = spriteData;
            this.xCount = spriteData.Boxes.Length;
            this.Width = this.Image.Width / xCount;
            this.Height = this.Image.Height;

            this.textureName = textureName;
            this.durrationInMs = durrationInSeconds * 1000;
        }

        public void HandleTimerTick(float dt)
        {
            this.time += dt;
            if (this.time > this.durrationInMs)
            {
                this.time -= this.durrationInMs;
            }

            this.xPos = (int)((this.time / this.durrationInMs) * this.xCount);
            this.xPos = Math.Min(this.xCount -1, this.xPos);
        }

        public void Draw(GraphicPanel2D panel, float x, float y, int pivotX, int pivotY, float zoom, float rotateZAngleInDegree, float rotateYAngleInDegree, bool showBoundingBox)
        {
            panel.PushMatrix();

            var m = Matrix4x4.Ident();
            
            m *= Matrix4x4.Translate(-pivotX, -pivotY, 0);          //Schritt 1: Verschiebe den PivotPunkt zum Nullpunkt
            m *= Matrix4x4.Scale(zoom, zoom, zoom);                 //Schritt 2: Skaliere
            m *= Matrix4x4.Rotate(rotateYAngleInDegree, 0, 1, 0);   //Schritt 3: Rotiere um Y
            m *= Matrix4x4.Rotate(rotateZAngleInDegree, 0, 0, 1);   //Schritt 4: Rotiere um Z
            m *= Matrix4x4.Translate(x, y, 0);                      //Schritt 5: Gehe zum Zielpunkt

            panel.MultTransformationMatrix(m);

            panel.DrawSprite(this.textureName, this.xCount, 1, this.xPos, 0, 0, 0, Width, Height, 0, true, Color.White);
            panel.DrawRectangle(Pens.Red, 0, 0, this.Width, this.Height);//Rahmen

            if (showBoundingBox)
            {
                var b = this.spriteData.Boxes[this.xPos];
                panel.DrawRectangle(Pens.Blue, b.X, b.Y, b.Width, b.Height);
            }
            

            panel.PopMatrix();
        }
    }
}
