using GameHelper;
using GameHelper.Simulation.RigidBodyTagging;
using GraphicMinimal;
using GraphicPanels;
using GraphicPanelWpf;

namespace AstroidsControl.Model
{
    //Klebt eine Sprite-Datei an zwei AnchorPoints
    class FireSprite : ITimerHandler
    {
        private SpriteImage sprite;
        private AnchorPoint anchorPoint1;
        private AnchorPoint anchorPoint2;
        private float loacalAnchorDistance;
        private Vector2D anchorMiddle;

        public FireSprite(string dataFolder, AnchorPoint anchorPoint1, AnchorPoint anchorPoint2)
        {
            this.sprite = new SpriteImage(dataFolder + "Fire.png", 5, 7, 32, 2, false, 140, 259);
            this.anchorPoint1 = anchorPoint1;
            this.anchorPoint2 = anchorPoint2;

            var localAnchor1 = new Vector2D(222, 259); //An diesem Pixel befindet sich die rechte untere Ecke vom Feuer
            var localAnchor2 = new Vector2D(59, 259);  //An diesem Pixel befindet sich die linke untere Ecke vom Feuer
            this.loacalAnchorDistance = (localAnchor1 - localAnchor2).Length();
            HandleTimerTick(0);
        }

        public void Draw(GraphicPanel2D panel)
        {
            this.sprite.Draw(panel, this.anchorMiddle);
        }

        public void HandleTimerTick(float dt)
        {
            this.sprite.HandleTimerTick(dt);

            var p1 = anchorPoint1.GetPosition();
            var p2 = anchorPoint2.GetPosition();

            float anchorDistance = (p2 - p1).Length();
            this.sprite.Zoom = anchorDistance / this.loacalAnchorDistance;
            this.sprite.RotateZAngleInDegree = 360 - Vector2D.Angle360(p1 - p2, new Vector2D(1, 0)) + 180;

            this.anchorMiddle = (p1 + p2) / 2;
        }
    }
}
