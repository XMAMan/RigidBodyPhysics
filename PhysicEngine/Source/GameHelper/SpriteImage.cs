using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;

namespace GameHelper
{
    //Hilft bei der Darstellung einer Spritedatei. Speichert welches Bild gerade ausgewählt ist und man kann das Bild auch über die Y- und Z-Achse drehen.
    public class SpriteImage : ITimerHandler
    {
        private string fileName;
        private int xCount;
        private int yCount;
                private float durrationInMs;
        private float time = 0;         //Geht von 0 bis durrationInMs
        private int spriteNr = 0;       //Aktuell ausgewähltes Bild
        private bool isRunning = true;
     
        public int ImageCount { get; private set; }//Für gewöhnlich ist imageCount=xCount*yCount aber wenn in der letzten Reihe Bilder fehlen, dann kann imageCount auch kleiner sein
        public int PivotX { get; private set; }
        public int PivotY { get; private set; }
        public Vector2D Pivot { get; private set; }
        public bool AnimateOneTime { get; private set; } = false; //true = Animation wird nur einmal angezeigt. Danach kann dann per Reset() zurück an den Anfang gesprungen werden; false = Animation beginnt von alleine wieder von vorne 
        public float Zoom { get; set; } = 1;
        public float RotateZAngleInDegree { get; set; } = 0;
        public float RotateYAngleInDegree { get; set; } = 0;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public event Action FinishIsReached; //Wird gerufen, wenn die Animation durchgelaufen ist, d.h. time == durrationInMs


        //fileName = Bilddateiname, wo alle Einzelbilder gleich groß sind
        //xCount = So viele Einzelbilder hat die Datei (Alle Einzelbilder stehen innerhalb einer Zeile)
        //pivotX/pivotY = Dieser Pixel ist der Nullpunkt/Drehpunkt eines Sprite-Einzelbildes
        public SpriteImage(string fileName, int xCount, int yCount, int imageCount, float durrationInSeconds, bool animateOneTime, int pivotX, int pivotY)
        {
            this.fileName = fileName;
            this.xCount = xCount;
            this.yCount = yCount;
            this.ImageCount = imageCount;
            this.durrationInMs = durrationInSeconds * 1000;
            var image = new Bitmap(fileName);
            this.Width = image.Width / xCount;
            this.Height = image.Height / yCount;
            this.PivotX = pivotX;
            this.PivotY = pivotY;
            this.Pivot = new Vector2D(pivotX, pivotY);
            this.AnimateOneTime = animateOneTime;
        }

        public void Reset()
        {
            this.time = 0;
            this.spriteNr = 0;
            this.isRunning = true;
        }

        public void HandleTimerTick(float dt)
        {
            if (this.isRunning == false) return;

            this.time += dt;
            if (this.time > this.durrationInMs)
            {
                if (this.AnimateOneTime)
                {
                    this.time = this.durrationInMs;
                    this.isRunning = false;
                }else
                {
                    this.time -= this.durrationInMs;
                }
                this.FinishIsReached?.Invoke();
            }

            this.spriteNr = (int)((this.time / this.durrationInMs) * this.ImageCount);
            this.spriteNr = Math.Min(this.ImageCount - 1, this.spriteNr);
        }

        //position = An dieser Stelle wird der Pivotpixel des aktiven EInzelbildes gezeichnet werden
        public void Draw(GraphicPanel2D panel, Vector2D position)
        {
            DrawSingleImage(panel, position, this.spriteNr);
        }

        public void DrawSingleImage(GraphicPanel2D panel, Vector2D position, int imageIndex)
        {
            panel.PushMatrix();

            panel.MultTransformationMatrix(GetLocalToWorldMatrix(position));
            panel.DrawSprite(this.fileName, this.xCount, this.yCount, imageIndex % this.xCount, imageIndex / this.xCount, 0, 0, Width, Height, 0, true, Color.White);

            panel.PopMatrix();
        }

        public Matrix4x4 GetLocalToWorldMatrix(Vector2D position)
        {
            var m = Matrix4x4.Ident();

            m *= Matrix4x4.Translate(-this.PivotX, -this.PivotY, 0);     //Schritt 1: Verschiebe den PivotPunkt zum Nullpunkt
            m *= Matrix4x4.Scale(this.Zoom, this.Zoom, this.Zoom);       //Schritt 2: Skaliere
            m *= Matrix4x4.Rotate(this.RotateYAngleInDegree, 0, 1, 0);   //Schritt 3: Rotiere um Y
            m *= Matrix4x4.Rotate(this.RotateZAngleInDegree, 0, 0, 1);   //Schritt 4: Rotiere um Z
            m *= Matrix4x4.Translate(position.X, position.Y, 0);         //Schritt 5: Gehe zum Zielpunkt

            return m;
        }

        //Ungedrehte BoundingBox
        public Rectangle GetBoundingBox(Vector2D position)
        {
            var points = GetCornerPoints(position);

            float minX = points.Min(x => x.X);
            float minY = points.Min(y => y.Y);
            float maxX = points.Max(x => x.X);
            float maxY = points.Max(y => y.Y);
            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }

        //BoundingBox welche gedreht wurde
        public Vector2D[] GetCornerPoints(Vector2D position)
        {
            var localToWorld = GetLocalToWorldMatrix(position);

            return new Vector2D[]
            {
                Matrix4x4.MultPosition(localToWorld, new Vector3D(0, 0, 0)).XY,
                Matrix4x4.MultPosition(localToWorld, new Vector3D(Width, 0, 0)).XY,
                Matrix4x4.MultPosition(localToWorld, new Vector3D(Width, Height, 0)).XY,
                Matrix4x4.MultPosition(localToWorld, new Vector3D(0, Height, 0)).XY
            };
        }
    }
}
